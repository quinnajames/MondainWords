using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;

namespace MondainDeploy
{
    public class QuizProcess
    {
        // todo: make labelTotalSolutions a non-ref term by changing UpdateTotalSolutionsLabelWhenCorrect
        public static string ProcessQuestion(ref Quiz currentQuiz, Label labelCurrentQuestion, ref Label labelTotalSolutions)
        {
            currentQuiz.CurrentQuestion = currentQuiz.QuizAlphaToWords[currentQuiz.QuestionNumber - 1];
            currentQuiz.CurrentAnswerList = currentQuiz.CurrentQuestion.Value;
            var returnstring = "#" + currentQuiz.QuestionNumber + ": " + currentQuiz.CurrentQuestion.Key;
            if (currentQuiz.IsBlankBingos)
                returnstring += MondainUI.Embolden("?");
            currentQuiz.ResetCurrentAnswerStats();
            labelTotalSolutions = UpdateTotalSolutionsLabelWhenCorrect(currentQuiz.GetBooleanAnswersThisQuestion()[0],
                currentQuiz.GetBooleanAnswersThisQuestion()[0] + currentQuiz.GetBooleanAnswersThisQuestion()[1], labelTotalSolutions);
            return returnstring;
        }

        public static Label UpdateTotalSolutionsLabelWhenCorrect(int questionsCorrect, int totalQuestions, Label labelTotalSolutions)
        {
            labelTotalSolutions.Text = questionsCorrect + " of " + totalQuestions;
            return labelTotalSolutions;
        }

        public static Dictionary<string, string> UpdateStats(ref Quiz currentQuiz)
        {
            

            var correct = currentQuiz.GetBooleanAnswersThisQuestion();

            currentQuiz.CorrectWordCount += correct[0];
            currentQuiz.IncorrectWordCount += correct[1];

            var cc = currentQuiz.CorrectAlphagramCount;
            var ic = currentQuiz.IncorrectAlphagramCount;
            var ccw = currentQuiz.CorrectWordCount;
            var icw = currentQuiz.IncorrectWordCount;

            var dictionary = new Dictionary<string, string>();
            dictionary.Add("labelStatsCorrectAlphagramFraction", cc.ToString() + '/' + (cc + ic));
            dictionary.Add("labelStatsCorrectAlphagramPercent", Math.Round(((double)cc / (cc + ic)) * 100, 2).ToString(CultureInfo.CurrentCulture) + "%");
            dictionary.Add("labelStatsCorrectWordFraction", ccw.ToString() + '/' + (ccw + icw));
            dictionary.Add("labelStatsCorrectWordPercent", Math.Round(((double)ccw / (ccw + icw)) * 100, 2).ToString(CultureInfo.CurrentCulture) + "%");

            return dictionary;
        }

        
    }
}