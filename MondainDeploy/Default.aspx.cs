using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using static MondainDeploy.MondainUI;

namespace MondainDeploy
{
    public partial class Default : Page
    {
        // Cleared with every postback.
        private Quiz _currentQuiz;
        private FullLexicon _fullLexicon;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsPostBack)
            {
                // Restore variables.
                _currentQuiz = (Quiz)ViewState["currentQuiz"];
                CurrentQuestionHistoryLabel.Text = (string)ViewState["answerSetText"];

            }
            else // if not postback
            {
                FillStaticDropdowns();
                MinDD.SelectedValue = Constants.WordLengthMinDefault.ToString();
                MaxDD.SelectedValue = Constants.WordLengthMaxDefault.ToString();
                MinProb.Text = Constants.ProbabilityMinDefault.ToString();
                MaxProb.Text = Constants.ProbabilityMaxDefault.ToString();
                TBQuizLength.Text = Constants.DefaultQuizLength.ToString();
            }
            // do this regardless
            TBQuizAnswer.Focus();
        }
        protected void Page_PreRender(object sender, EventArgs e)
        {
            ViewState["currentQuiz"] = _currentQuiz;
            ViewState["answerSetText"] = CurrentQuestionHistoryLabel.Text;
        }

        private void FillStaticDropdowns()
        {
            foreach (int element in Constants.WordLengths)
            {
                var elString = element.ToString();
                MinDD.Items.Add(elString);
                MaxDD.Items.Add(elString);
            }
        }

        private void ClearTextControl(ITextControl label)
        {
            label.Text = string.Empty;
        }

        protected void cmdStartQuiz_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;
            ClearTextControl(CurrentQuestionHistoryLabel);
            _currentQuiz = InitializeCurrentQuiz();
            LabelCurrentQuestion.Text = $"#{_currentQuiz.QuestionNumber}: {_currentQuiz.CurrentQuestion.Key}";
            QuizProcess.ProcessQuestion(ref _currentQuiz, LabelCurrentQuestion, ref LabelTotalSolutions);
        }
        private Quiz InitializeCurrentQuiz()
        {
            var quizLengthValue = TryIntParseWithDefault(TBQuizLength.Text, Constants.DefaultQuizLength);
            var minValue = TryIntParseWithDefault(MinDD.SelectedValue, Constants.WordLengthMinDefault);
            var maxValue = TryIntParseWithDefault(MaxDD.SelectedValue, Constants.WordLengthMaxDefault);
            var minProbValue = TryIntParseWithDefault(MinProb.Text, Constants.ProbabilityMinDefault);
            var maxProbValue = TryIntParseWithDefault(MaxProb.Text, Constants.ProbabilityMaxDefault);

            System.Configuration.Configuration rootWebConfig =
                System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/MondainDeploy");
            if (rootWebConfig.ConnectionStrings.ConnectionStrings.Count > 0)
            {
                var connString = rootWebConfig.ConnectionStrings.ConnectionStrings["LocalSQLExpressConnectionString"];
                _fullLexicon = new FullLexicon(new LexTableWrapper(connString, true));
            }

            ClearTextControl(CurrentStatus);
            CurrentStatus.Text = ("test");
            CurrentStatus.Text = PostpendLineTo(CurrentStatus.Text, "Lexicon words: " + _fullLexicon.GetWordCount());
            CurrentStatus.Text = PostpendLineTo(CurrentStatus.Text, "Alphagrams: " + _fullLexicon.GetAlphagramCount());


            List<KeyValuePair<string, List<string>>> tempQuizAlphaToWords;
            bool isBlankBingos = BlankBingoCheck.Checked;
            bool usingLexSymbols = LexSymbolCheck.Checked;
            if (!isBlankBingos)
            {
                   tempQuizAlphaToWords = _fullLexicon.GetRandomQuizEntries(quizLengthValue,
                    new Random(), minValue, maxValue, minProbValue, maxProbValue);             
            }

            else
                tempQuizAlphaToWords = _fullLexicon.GetBlankBingoEntries(quizLengthValue, new Random(), minValue, maxValue);

            if (tempQuizAlphaToWords.Count != quizLengthValue)
                quizLengthValue = tempQuizAlphaToWords.Count;

            var quiz = new Quiz(quizLengthValue, tempQuizAlphaToWords, isBlankBingos, usingLexSymbols);
            CurrentStatus.Text = PostpendLineTo(CurrentStatus.Text, "Initialized quiz with " + quiz.QuizLength + " questions.");
            CurrentStatus.Text = PostpendLineTo(CurrentStatus.Text, "Using lexicon symbols: " + usingLexSymbols.ToString());

            return quiz;
        }

        // workflow per question

        private void MarkQuestionCorrect()
        {
            _currentQuiz.IncrementCounts(true);
            AdvanceQuestion();
        }
        private void MarkQuestionMissed()
        {
            _currentQuiz.IncrementCounts(false);
            foreach (var word in _currentQuiz.CurrentAnswerList)
            {
                CurrentQuestionHistoryLabel.Text = PrependLineTo(CurrentQuestionHistoryLabel.Text, FormatMissedRightAnswer(GetWordWithLexiconSymbols(word)));
            }
            CurrentQuestionHistoryLabel.Text = PrependLineTo(CurrentQuestionHistoryLabel.Text, "Question " + _currentQuiz.QuestionNumber + Embolden(" incorrect") + "!");
            MoveCurrentQuestionToAnswerHistory();
        }
        private void MoveCurrentQuestionToAnswerHistory()
        {
            //todo: Add an option not to show incorrect responses in the answer history
            AnswerHistory.Text = PrependStringTo(AnswerHistory.Text, CurrentQuestionHistoryLabel.Text);
            ClearTextControl(CurrentQuestionHistoryLabel);
        }
        private void AdvanceQuestion()
        {
            if (_currentQuiz.Finished)
                CurrentStatus.Text = PrependLineTo(CurrentStatus.Text, Constants.QuizAlreadyFinishedText);
            else
            {
                var statskvp = QuizProcess.UpdateStats(ref _currentQuiz);
                
                Label_StatsCorrectAlphagramFraction.Text = statskvp["labelStatsCorrectAlphagramFraction"];
                Label_StatsCorrectAlphagramPercent.Text = statskvp["labelStatsCorrectAlphagramPercent"];
                Label_StatsCorrectWordFraction.Text = statskvp["labelStatsCorrectWordFraction"];
                Label_StatsCorrectWordPercent.Text = statskvp["labelStatsCorrectWordPercent"];


                if (_currentQuiz.QuestionNumber < _currentQuiz.QuizLength)
                {
                    _currentQuiz.QuestionNumber++;
                    QuizProcess.ProcessQuestion(ref _currentQuiz, LabelCurrentQuestion, ref LabelTotalSolutions);
                    LabelCurrentQuestion.Text = $"#{_currentQuiz.QuestionNumber}: {_currentQuiz.CurrentQuestion.Key}";
                }
                else
                    EndQuiz();
            }
        }

        

        // todo: Decide whether best renamed to GetFormattedWord or kept like this
        private string GetWordWithLexiconSymbols(string word)
        {
            // todo: rename CurrentQuizAnswerStatsList
            foreach (var wd in _currentQuiz.CurrentQuizAnswerStatsList)
            {
                if (wd.Word == word)
                {
                    return wd.WordDisplayString;
                }
            }
            return word;
        }

        // todo: Redundancy check on this
        protected void ClearQuestionHistoryOnFirstSubmit()
        {
            ClearTextControl(CurrentQuestionHistoryLabel);
        }

        protected void SubmitAnswerButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            var submittedAnswer = TBQuizAnswer.Text.ToUpper();
            ClearTextControl(TBQuizAnswer);

            if (CurrentQuestionHistoryLabel.Text == Constants.AnswerSetDefaultText)
                ClearQuestionHistoryOnFirstSubmit();

            if (_currentQuiz.Finished)
            {
                CurrentStatus.Text = PrependLineTo(CurrentStatus.Text, Constants.QuizAlreadyFinishedText);
                return;
            }
            if (_currentQuiz.CurrentAnswerList.Contains(submittedAnswer))
            {
                
                _currentQuiz.SetWordAsCorrect(submittedAnswer);
                _currentQuiz.CurrentAnswerList.Remove(submittedAnswer);
                CurrentQuestionHistoryLabel.Text = PrependLineTo(CurrentQuestionHistoryLabel.Text, 
                    GetWordWithLexiconSymbols(submittedAnswer));
                // todo: Look at whether UpdateTotalSolutionsLabelWhenCorrect should return a string or a label
                var correctAnswerCount = _currentQuiz.GetBooleanAnswersThisQuestion()[0];
                var incorrectAnswerCount = _currentQuiz.GetBooleanAnswersThisQuestion()[1];
                LabelTotalSolutions.Text = QuizProcess.UpdateTotalSolutionsLabelWhenCorrect(correctAnswerCount,
                    correctAnswerCount + incorrectAnswerCount, LabelTotalSolutions).Text;
            }
            else
                CurrentQuestionHistoryLabel.Text = PrependLineTo(CurrentQuestionHistoryLabel.Text, Strike(GetWordWithLexiconSymbols(submittedAnswer)));

            if (_currentQuiz.CurrentAnswerList.Count != 0) return;
            CurrentQuestionHistoryLabel.Text = PrependLineTo(CurrentQuestionHistoryLabel.Text, "Question " + _currentQuiz.QuestionNumber + Embolden(" correct") + "!");
            MoveCurrentQuestionToAnswerHistory();
            MarkQuestionCorrect();
        }



        protected void MarkMissedButton_Click(object sender, EventArgs e)
        {
            MarkQuestionMissed();
            AdvanceQuestion();
        }

        protected void EndQuiz()
        {
            const string quizFinishedText = "Quiz complete!"; 
            _currentQuiz.Finished = true;
            CurrentStatus.Text = PrependLineTo(CurrentStatus.Text, quizFinishedText);
            LabelCurrentQuestion.Text = quizFinishedText;
            // todo: add params override for CTC
            ClearTextControl(LabelTotalSolutions);
            ClearTextControl(CurrentQuestionHistoryLabel);
        }


    }

    public partial class Default
    {
        protected void customQuizLength_ServerValidate(object source, ServerValidateEventArgs args)
        {
            try
            {
                var quizLengthValue = TryIntParseWithDefault(TBQuizLength.Text, Constants.DefaultQuizLength);
                var minProbValue = TryIntParseWithDefault(MinProb.Text, Constants.ProbabilityMinDefault);
                var maxProbValue = TryIntParseWithDefault(MaxProb.Text, Constants.ProbabilityMaxDefault);

                args.IsValid = quizLengthValue - 1 <= maxProbValue - minProbValue;
            }
            catch
            {
                args.IsValid = false;
            }
        }
    }
}