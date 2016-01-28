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
            ProcessQuestion();
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
            CurrentStatus.Text = PostpendLineTo(CurrentStatus.Text, "Lexicon words: " + _fullLexicon.GetWordCount().ToString());
            CurrentStatus.Text = PostpendLineTo(CurrentStatus.Text, "Alphagrams: " + _fullLexicon.GetAlphagramCount().ToString());

            bool isBlankBingos = BlankBingoCheck.Checked;
            List<KeyValuePair<string, List<string>>> tempQuizATW = new List<KeyValuePair<string, List<string>>>();
            if (!isBlankBingos)
                tempQuizATW = _fullLexicon.GetRandomQuizEntries(quizLengthValue,
                    new Random(), minValue, maxValue, minProbValue, maxProbValue);
            else
                tempQuizATW = _fullLexicon.GetBlankBingoEntries(quizLengthValue, new Random(), minValue, maxValue);

            if (tempQuizATW.Count != quizLengthValue)
                quizLengthValue = tempQuizATW.Count;

            var quiz = new Quiz(quizLengthValue, tempQuizATW, isBlankBingos);
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
                CurrentQuestionHistoryLabel.Text = PrependLineTo(CurrentQuestionHistoryLabel.Text, Embolden(Italicize(word)));
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
                    ProcessQuestion();
                }
                else
                    EndQuiz();
            }
        }
        private void ProcessQuestion()
        {
            _currentQuiz.CurrentQuestion = _currentQuiz.QuizAlphaToWords[_currentQuiz.QuestionNumber - 1];
            _currentQuiz.CurrentAnswerList = _currentQuiz.CurrentQuestion.Value;
            LabelCurrentQuestion.Text = "#" + _currentQuiz.QuestionNumber + ": " + _currentQuiz.CurrentQuestion.Key;
            if (_currentQuiz.IsBlankBingos)
                LabelCurrentQuestion.Text += Embolden("?");
            _currentQuiz.ResetCurrentAnswerWordCount();
            UpdateTotalSolutionsLabelWhenCorrect(_currentQuiz.GetBooleanAnswersThisQuestion()[0],
        _currentQuiz.GetBooleanAnswersThisQuestion()[0] + _currentQuiz.GetBooleanAnswersThisQuestion()[1]); ;

        }
        private void UpdateTotalSolutionsLabelWhenCorrect(int questionsCorrect, int totalQuestions)
        {
            LabelTotalSolutions.Text = questionsCorrect.ToString() + " of " + totalQuestions.ToString();
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
            Label_StatsCorrectAlphagramPercent.Text = Math.Round(((double)cc / (cc + ic)) * 100, 2).ToString(CultureInfo.InvariantCulture) + "%";
            Label_StatsCorrectWordFraction.Text = ccw.ToString() + '/' + (ccw + icw);
            Label_StatsCorrectWordPercent.Text = Math.Round(((double)ccw / (ccw + icw)) * 100, 2).ToString(CultureInfo.InvariantCulture) + "%";

        }

        // workflow per answer
        protected void SubmitAnswerButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;
            var submittedAnswer = TBQuizAnswer.Text.ToUpper();
            ClearTextControl(TBQuizAnswer);

            var AnswerSetDefaultText = "Answers displayed here";
            if (CurrentQuestionHistoryLabel.Text == AnswerSetDefaultText)
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
                UpdateTotalSolutionsLabelWhenCorrect(_currentQuiz.GetBooleanAnswersThisQuestion()[0],
                    _currentQuiz.GetBooleanAnswersThisQuestion()[0] + _currentQuiz.GetBooleanAnswersThisQuestion()[1]);
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

        // utility functions

//        private string Italicize(string str) => "<i>" + str + "</i>";
//        private string Embolden(string str) => "<strong>" + str + "</strong>";
//        private string Strike(string str) => "<span style=\"color:lightgray\"><del>" + str + "</del></span>";



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