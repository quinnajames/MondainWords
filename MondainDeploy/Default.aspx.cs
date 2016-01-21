using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MondainDeploy
{
    public partial class Default : Page
    {
        // Cleared with every postback.
        private Quiz currentQuiz;
        private FullLexicon tempFL;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsPostBack)
            {
                // Restore variables.
                currentQuiz = (Quiz)ViewState["currentQuiz"];
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
            ViewState["currentQuiz"] = currentQuiz;
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
        

        protected void cmdStartQuiz_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;
            ResetQuestion();
            currentQuiz = InitializeCurrentQuiz();
            ProcessQuestion();
        }
        private Quiz InitializeCurrentQuiz()
        {
            int quizLengthValue = MondainUI.TryParseWithDefault(TBQuizLength.Text, Constants.DefaultQuizLength);
            int minValue = MondainUI.TryParseWithDefault(MinDD.SelectedValue, Constants.WordLengthMinDefault);
            int maxValue = MondainUI.TryParseWithDefault(MaxDD.SelectedValue, Constants.WordLengthMaxDefault);
            int minProbValue = MondainUI.TryParseWithDefault(MinProb.Text, Constants.ProbabilityMinDefault);
            int maxProbValue = MondainUI.TryParseWithDefault(MaxProb.Text, Constants.ProbabilityMaxDefault);

            // This is only declared to make the function call at the bottom coherent.
            // May be a canary in the coalmine# here.
            int temp_questionNumber = 1;

            System.Configuration.Configuration rootWebConfig =
                System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/MondainDeploy");
            System.Configuration.ConnectionStringSettings connString;
            if (rootWebConfig.ConnectionStrings.ConnectionStrings.Count > 0)
            {
                connString =
                    rootWebConfig.ConnectionStrings.ConnectionStrings["LocalSQLExpressConnectionString"];
                tempFL = new FullLexicon(new LexTableWrapper(connString, true));
            }

            
            CurrentStatus.Text = "";
            CurrentStatus.Text = PostpendLine(CurrentStatus.Text, "Lexicon words: " + tempFL.GetWordCount().ToString());
            CurrentStatus.Text = PostpendLine(CurrentStatus.Text, "Alphagrams: " + tempFL.GetAlphagramCount().ToString());

            bool isBlankBingos = BlankBingoCheck.Checked;
            List<KeyValuePair<string, List<string>>> tempQuizATW = new List<KeyValuePair<string, List<string>>>();
            if (!isBlankBingos)
                tempQuizATW = tempFL.GetRandomQuizEntries(quizLengthValue,
                    new Random(), minValue, maxValue, minProbValue, maxProbValue);
            else
                tempQuizATW = tempFL.GetBlankBingoEntries(quizLengthValue, new Random(), minValue, maxValue);

            if (tempQuizATW.Count != quizLengthValue)
                quizLengthValue = tempQuizATW.Count;

            var quiz = new Quiz(quizLengthValue, temp_questionNumber, tempQuizATW, isBlankBingos);
            CurrentStatus.Text = PostpendLine(CurrentStatus.Text, "Initialized quiz with " + quiz.QuizLength + " questions. ");
            return quiz;
        }

        // workflow per question
        private void ResetQuestion() => CurrentQuestionHistoryLabel.Text = "";

        private void MarkQuestionCorrect()
        {
            currentQuiz.IncrementCounts(true);
            AdvanceQuestion();
        }
        private void MarkQuestionMissed()
        {
            currentQuiz.IncrementCounts(false);
            foreach (var word in currentQuiz.CurrentAnswerList)
            {
                CurrentQuestionHistoryLabel.Text = PrependLine(CurrentQuestionHistoryLabel.Text, Embolden(Italicize(word)));
            }
            CurrentQuestionHistoryLabel.Text = PrependLine(CurrentQuestionHistoryLabel.Text, "Question " + currentQuiz.QuestionNumber + Embolden(" incorrect") + "!");
            MoveCurrentQuestionToAnswerHistory();
        }
        private void MoveCurrentQuestionToAnswerHistory()
        {
            AnswerHistory.Text = PrependString(AnswerHistory.Text, CurrentQuestionHistoryLabel.Text);
            CurrentQuestionHistoryLabel.Text = "";
        }
        private void AdvanceQuestion()
        {
            if (currentQuiz.Finished)
                CurrentStatus.Text = PrependLine(CurrentStatus.Text, "Quiz is already finished!");
            else
            {
                UpdateStats();
                if (currentQuiz.QuestionNumber < currentQuiz.QuizLength)
                {
                    currentQuiz.QuestionNumber++;
                    ProcessQuestion();
                }
                else
                    EndQuiz();
            }
        }
        private void ProcessQuestion()
        {
            currentQuiz.CurrentQuestion = currentQuiz.QuizAlphaToWords[currentQuiz.QuestionNumber - 1];
            currentQuiz.CurrentAnswerList = currentQuiz.CurrentQuestion.Value;
            LabelCurrentQuestion.Text = "#" + currentQuiz.QuestionNumber + ": " + currentQuiz.CurrentQuestion.Key;
            if (currentQuiz.IsBlankBingos)
                LabelCurrentQuestion.Text += Embolden("?");
            currentQuiz.ResetCurrentAnswerWordCount();
            UpdateTotalSolutionsLabelWhenCorrect(currentQuiz.GetBooleanAnswersThisQuestion()[0],
        currentQuiz.GetBooleanAnswersThisQuestion()[0] + currentQuiz.GetBooleanAnswersThisQuestion()[1]); ;

        }
        private void UpdateTotalSolutionsLabelWhenCorrect(int questionsCorrect, int totalQuestions)
        {
            LabelTotalSolutions.Text = questionsCorrect.ToString() + " of " + totalQuestions.ToString();
        }
        private void UpdateStats()
        {
            var correct = currentQuiz.GetBooleanAnswersThisQuestion();
            
            currentQuiz.CorrectWordCount += correct[0];
            currentQuiz.IncorrectWordCount += correct[1];

            var cc = currentQuiz.CorrectAlphagramCount;
            var ic = currentQuiz.IncorrectAlphagramCount;
            var ccw = currentQuiz.CorrectWordCount;
            var icw = currentQuiz.IncorrectWordCount;

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
            TBQuizAnswer.Text = "";

            var AnswerSetDefaultText = "Answers displayed here";
            if (CurrentQuestionHistoryLabel.Text == AnswerSetDefaultText)
                CurrentQuestionHistoryLabel.Text = "";

            if (currentQuiz.Finished)
            {
                CurrentStatus.Text = PrependLine(CurrentStatus.Text, "Error: Quiz is already completed!");
                return;
            }
            if (currentQuiz.CurrentAnswerList.Contains(submittedAnswer))
            {
                currentQuiz.SetWordAsCorrect(submittedAnswer);
                currentQuiz.CurrentAnswerList.Remove(submittedAnswer);
                CurrentQuestionHistoryLabel.Text = PrependLine(CurrentQuestionHistoryLabel.Text, submittedAnswer);
                UpdateTotalSolutionsLabelWhenCorrect(currentQuiz.GetBooleanAnswersThisQuestion()[0],
                    currentQuiz.GetBooleanAnswersThisQuestion()[0] + currentQuiz.GetBooleanAnswersThisQuestion()[1]);
            }
            else
                CurrentQuestionHistoryLabel.Text = PrependLine(CurrentQuestionHistoryLabel.Text, Strike(submittedAnswer));

            if (currentQuiz.CurrentAnswerList.Count != 0) return;
            CurrentQuestionHistoryLabel.Text = PrependLine(CurrentQuestionHistoryLabel.Text, "Question " + currentQuiz.QuestionNumber + Embolden(" correct") + "!");
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
            currentQuiz.Finished = true;
            CurrentStatus.Text = PrependLine(CurrentStatus.Text, "Quiz complete!");
            LabelCurrentQuestion.Text = "Quiz complete!";
            LabelTotalSolutions.Text = "";
            CurrentQuestionHistoryLabel.Text = "";
        }

        // utility functions
        string PrependString(string inputString, string toBePrepended) => toBePrepended + inputString;
        string PrependLine(string inputString, string toBePrepended) => PrependString(inputString, toBePrepended + "<br />");
        string PostpendLine(string inputString, string toBePostpended) => PrependString(toBePostpended + "<br />", inputString);
        private string Italicize(string str) => "<i>" + str + "</i>";
        private string Embolden(string str) => "<strong>" + str + "</strong>";
        private string Strike(string str) => "<span style=\"color:lightgray\"><del>" + str + "</del></span>";



    }

    public partial class Default
    {
        protected void customQuizLength_ServerValidate(object source, ServerValidateEventArgs args)
        {
            try
            {
                int quizLengthValue = MondainUI.TryParseWithDefault(TBQuizLength.Text, Constants.DefaultQuizLength);
                int minProbValue = MondainUI.TryParseWithDefault(MinProb.Text, Constants.ProbabilityMinDefault);
                int maxProbValue = MondainUI.TryParseWithDefault(MaxProb.Text, Constants.ProbabilityMaxDefault);

                args.IsValid = quizLengthValue - 1 <= maxProbValue - minProbValue;
            }
            catch
            {
                args.IsValid = false;
            }
        }
    }
}