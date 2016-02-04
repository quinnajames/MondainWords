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
            QuizProcess.ProcessQuestion(ref _currentQuiz, LabelCurrentQuestion, ref LabelTotalSolutions);
        }
        private Quiz InitializeCurrentQuiz()
        {
            int quizLengthValue = TryParseWithDefault(TBQuizLength.Text, Constants.DefaultQuizLength);
            int minValue = TryParseWithDefault(MinDD.SelectedValue, Constants.WordLengthMinDefault);
            int maxValue = TryParseWithDefault(MaxDD.SelectedValue, Constants.WordLengthMaxDefault);
            int minProbValue = TryParseWithDefault(MinProb.Text, Constants.ProbabilityMinDefault);
            int maxProbValue = TryParseWithDefault(MaxProb.Text, Constants.ProbabilityMaxDefault);

            System.Configuration.Configuration rootWebConfig =
                System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/MondainDeploy");
            if (rootWebConfig.ConnectionStrings.ConnectionStrings.Count > 0)
            {
                var connString = rootWebConfig.ConnectionStrings.ConnectionStrings["LocalSQLExpressConnectionString"];
                _fullLexicon = new FullLexicon(new LexTableWrapper(connString, true));
            }

            ClearTextControl(CurrentStatus);
            CurrentStatus.Text = PostpendLineTo(CurrentStatus.Text, "Lexicon words: " + _fullLexicon.GetWordCount());
            CurrentStatus.Text = PostpendLineTo(CurrentStatus.Text, "Alphagrams: " + _fullLexicon.GetAlphagramCount());


            List<KeyValuePair<string, List<string>>> tempQuizAlphaToWords;
            bool isBlankBingos = BlankBingoCheck.Checked;
            if (!isBlankBingos)
            {
                   tempQuizAlphaToWords = _fullLexicon.GetRandomQuizEntries(quizLengthValue,
                    new Random(), minValue, maxValue, minProbValue, maxProbValue);             
            }

            else
                tempQuizAlphaToWords = _fullLexicon.GetBlankBingoEntries(quizLengthValue, new Random(), minValue, maxValue);

            if (tempQuizAlphaToWords.Count != quizLengthValue)
                quizLengthValue = tempQuizAlphaToWords.Count;

            var quiz = new Quiz(quizLengthValue, tempQuizAlphaToWords, isBlankBingos);
            CurrentStatus.Text = PostpendLineTo(CurrentStatus.Text, "Initialized quiz with " + quiz.QuizLength + " questions. ");
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
                CurrentQuestionHistoryLabel.Text = PrependLineTo(CurrentQuestionHistoryLabel.Text, MondainUI.FormatMissedRightAnswer(word));
            }
            CurrentQuestionHistoryLabel.Text = PrependLineTo(CurrentQuestionHistoryLabel.Text, "Question " + _currentQuiz.QuestionNumber + Embolden(" incorrect") + "!");
            MoveCurrentQuestionToAnswerHistory();
        }
        private void MoveCurrentQuestionToAnswerHistory()
        {
            AnswerHistory.Text = PrependStringTo(AnswerHistory.Text, CurrentQuestionHistoryLabel.Text);
            ClearTextControl(CurrentQuestionHistoryLabel);
        }
        private void AdvanceQuestion()
        {
            if (_currentQuiz.Finished)
                CurrentStatus.Text = PrependLineTo(CurrentStatus.Text, "Quiz is already finished!");
            else
            {
                UpdateStats();
                if (_currentQuiz.QuestionNumber < _currentQuiz.QuizLength)
                {
                    _currentQuiz.QuestionNumber++;
                    QuizProcess.ProcessQuestion(ref _currentQuiz, LabelCurrentQuestion, ref LabelTotalSolutions);
                }
                else
                    EndQuiz();
            }
        }

        private void UpdateStats()
        {
            var correct = _currentQuiz.GetBooleanAnswersThisQuestion();
            
            _currentQuiz.CorrectWordCount += correct[0];
            _currentQuiz.IncorrectWordCount += correct[1];

            var cc = _currentQuiz.CorrectAlphagramCount;
            var ic = _currentQuiz.IncorrectAlphagramCount;
            var ccw = _currentQuiz.CorrectWordCount;
            var icw = _currentQuiz.IncorrectWordCount;

            Label_StatsCorrectAlphagramFraction.Text = cc.ToString() + '/' + (cc + ic);
            Label_StatsCorrectAlphagramPercent.Text = Math.Round(((double)cc / (cc + ic)) * 100, 2).ToString(CultureInfo.CurrentCulture) + "%";
            Label_StatsCorrectWordFraction.Text = ccw.ToString() + '/' + (ccw + icw);
            Label_StatsCorrectWordPercent.Text = Math.Round(((double)ccw / (ccw + icw)) * 100, 2).ToString(CultureInfo.CurrentCulture) + "%";

        }

        // workflow per answer
        protected void SubmitAnswerButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;
            var submittedAnswer = TBQuizAnswer.Text.ToUpper();
            ClearTextControl(TBQuizAnswer);

            const string answerSetDefaultText = "Answers displayed here";
            if (CurrentQuestionHistoryLabel.Text == answerSetDefaultText)
                ClearTextControl(CurrentQuestionHistoryLabel);

            if (_currentQuiz.Finished)
            {
                CurrentStatus.Text = PrependLineTo(CurrentStatus.Text, "Error: Quiz is already completed!");
                return;
            }
            if (_currentQuiz.CurrentAnswerList.Contains(submittedAnswer))
            {
                _currentQuiz.SetWordAsCorrect(submittedAnswer);
                _currentQuiz.CurrentAnswerList.Remove(submittedAnswer);
                CurrentQuestionHistoryLabel.Text = PrependLineTo(CurrentQuestionHistoryLabel.Text, submittedAnswer);
                // todo: Look at whether UpdateTotalSolutionsLabelWhenCorrect should return a string or a label
                LabelTotalSolutions.Text = QuizProcess.UpdateTotalSolutionsLabelWhenCorrect(_currentQuiz.GetBooleanAnswersThisQuestion()[0],
                    _currentQuiz.GetBooleanAnswersThisQuestion()[0] + _currentQuiz.GetBooleanAnswersThisQuestion()[1], LabelTotalSolutions).Text;
            }
            else
                CurrentQuestionHistoryLabel.Text = PrependLineTo(CurrentQuestionHistoryLabel.Text, Strike(submittedAnswer));

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
            _currentQuiz.Finished = true;
            CurrentStatus.Text = PrependLineTo(CurrentStatus.Text, "Quiz complete!");
            LabelCurrentQuestion.Text = "Quiz complete!";
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
                int quizLengthValue = TryParseWithDefault(TBQuizLength.Text, Constants.DefaultQuizLength);
                int minProbValue = TryParseWithDefault(MinProb.Text, Constants.ProbabilityMinDefault);
                int maxProbValue = TryParseWithDefault(MaxProb.Text, Constants.ProbabilityMaxDefault);

                args.IsValid = quizLengthValue - 1 <= maxProbValue - minProbValue;
            }
            catch
            {
                args.IsValid = false;
            }
        }
    }
}