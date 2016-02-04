using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace MondainDeploy
{
    public class QuizProcess
    {
        public static string ProcessQuestion(ref Quiz currentQuiz, System.Web.UI.WebControls.Label labelCurrentQuestion, ref System.Web.UI.WebControls.Label labelTotalSolutions)
        {
            currentQuiz.CurrentQuestion = currentQuiz.QuizAlphaToWords[currentQuiz.QuestionNumber - 1];
            currentQuiz.CurrentAnswerList = currentQuiz.CurrentQuestion.Value;
            var returnstring = "#" + currentQuiz.QuestionNumber + ": " + currentQuiz.CurrentQuestion.Key;
            if (currentQuiz.IsBlankBingos)
                returnstring += MondainUI.Embolden("?");
            currentQuiz.ResetCurrentAnswerWordCount();
            labelTotalSolutions = UpdateTotalSolutionsLabelWhenCorrect(currentQuiz.GetBooleanAnswersThisQuestion()[0],
                currentQuiz.GetBooleanAnswersThisQuestion()[0] + currentQuiz.GetBooleanAnswersThisQuestion()[1], labelTotalSolutions);
            return returnstring;
        }

        public static System.Web.UI.WebControls.Label UpdateTotalSolutionsLabelWhenCorrect(int questionsCorrect, int totalQuestions, System.Web.UI.WebControls.Label labelTotalSolutions)
        {
            labelTotalSolutions.Text = questionsCorrect + " of " + totalQuestions;
            return labelTotalSolutions;
        }

        public static void UpdateStats(ref Quiz currentQuiz, ref Label labelStatsCorrectAlphagramFraction, ref Label labelStatsCorrectAlphagramPercent, ref Label labelStatsCorrectWordFraction, ref Label labelStatsCorrectWordPercent)
        {
            var correct = currentQuiz.GetBooleanAnswersThisQuestion();

            currentQuiz.CorrectWordCount += correct[0];
            currentQuiz.IncorrectWordCount += correct[1];

            var cc = currentQuiz.CorrectAlphagramCount;
            var ic = currentQuiz.IncorrectAlphagramCount;
            var ccw = currentQuiz.CorrectWordCount;
            var icw = currentQuiz.IncorrectWordCount;

            labelStatsCorrectAlphagramFraction.Text = cc.ToString() + '/' + (cc + ic);
            labelStatsCorrectAlphagramPercent.Text = Math.Round(((double)cc / (cc + ic)) * 100, 2).ToString(CultureInfo.CurrentCulture) + "%";
            labelStatsCorrectWordFraction.Text = ccw.ToString() + '/' + (ccw + icw);
            labelStatsCorrectWordPercent.Text = Math.Round(((double)ccw / (ccw + icw)) * 100, 2).ToString(CultureInfo.CurrentCulture) + "%";

        }
    }
}