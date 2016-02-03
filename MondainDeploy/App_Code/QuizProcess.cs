using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MondainDeploy
{
    public static class QuizProcess
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
    }
}