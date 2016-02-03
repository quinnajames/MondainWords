using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MondainDeploy
{
    public static class QuizProcess
    {
        public static string ProcessQuestion(ref Quiz _currentQuiz, global::System.Web.UI.WebControls.Label LabelCurrentQuestion, ref global::System.Web.UI.WebControls.Label LabelTotalSolutions)
        {
            var returnstring = LabelCurrentQuestion.Text;
            _currentQuiz.CurrentQuestion = _currentQuiz.QuizAlphaToWords[_currentQuiz.QuestionNumber - 1];
            _currentQuiz.CurrentAnswerList = _currentQuiz.CurrentQuestion.Value;
            returnstring = "#" + _currentQuiz.QuestionNumber + ": " + _currentQuiz.CurrentQuestion.Key;
            if (_currentQuiz.IsBlankBingos)
                returnstring += MondainUI.Embolden("?");
            _currentQuiz.ResetCurrentAnswerWordCount();
            LabelTotalSolutions = UpdateTotalSolutionsLabelWhenCorrect(_currentQuiz.GetBooleanAnswersThisQuestion()[0],
                _currentQuiz.GetBooleanAnswersThisQuestion()[0] + _currentQuiz.GetBooleanAnswersThisQuestion()[1], LabelTotalSolutions);
            return returnstring;
        }

        public static global::System.Web.UI.WebControls.Label UpdateTotalSolutionsLabelWhenCorrect(int questionsCorrect, int totalQuestions, global::System.Web.UI.WebControls.Label LabelTotalSolutions)
        {
            LabelTotalSolutions.Text = questionsCorrect + " of " + totalQuestions;
            return LabelTotalSolutions;
        }
    }
}