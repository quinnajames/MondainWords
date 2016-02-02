using System;
using System.Collections.Generic;
using System.Linq;

namespace MondainDeploy
{
    /// <summary>
    /// Summary description for Quiz
    /// </summary>
    [Serializable]
    public class Quiz
    {
        public int QuizLength { get; set; }
        public int QuestionNumber { get; set; }
        public KeyValuePair<string, List<string>> CurrentQuestion { get; set; }
        public List<WordData> CurrentAnswerResultsList;

        public List<QuizAnswer> CurrentQuizAnswerStatsList;

        public List<string> CurrentAnswerList { get; set; }

        public List<KeyValuePair<string, List<string>>> QuizAlphaToWords { get; set; }

        public int CorrectAlphagramCount;
        public int IncorrectAlphagramCount;
        public int CorrectWordCount, IncorrectWordCount, CurrentAnswerWordCount;
        public bool Finished { get; set; }
        public bool IsBlankBingos { get; set; }
        /// <summary>
        /// Creates a new quiz initialized at 1.
        /// </summary>

        public Quiz(int quizLength, List<KeyValuePair<string, List<string>>> quizAlphaToWords, bool isBlankBingos, int questionNumber = 1)
        {
            QuizLength = quizLength;
            QuestionNumber = questionNumber;
            QuizAlphaToWords = quizAlphaToWords;
            IsBlankBingos = isBlankBingos;
            CurrentQuestion = QuizAlphaToWords[QuestionNumber - 1];
            CurrentAnswerList = QuizAlphaToWords[QuestionNumber - 1].Value;
            CurrentAnswerWordCount = QuizAlphaToWords[QuestionNumber - 1].Value.Count;
            CorrectAlphagramCount = IncorrectAlphagramCount = CorrectWordCount = IncorrectWordCount = 0;
            Finished = false;

            CurrentQuizAnswerStatsList = new List<QuizAnswer>();
            foreach (var str in CurrentAnswerList)
            {
                CurrentQuizAnswerStatsList.Add(new QuizAnswer(str, false));
            }

        }

        public void SetWordAsCorrect(string word)
        {
            foreach (var kvp in CurrentQuizAnswerStatsList.Where(kvp => kvp.Word == word))
            {
                kvp.IsCorrect = true;
            }
        }

        public void ResetCurrentAnswerWordCount()
        {
            CurrentAnswerWordCount = QuizAlphaToWords[QuestionNumber - 1].Value.Count;

            CurrentQuizAnswerStatsList = new List<QuizAnswer>();
            foreach (var str in CurrentAnswerList)
            {
                CurrentQuizAnswerStatsList.Add(new QuizAnswer(str, false));
            }

        }

        public void IncrementCounts(bool isCorrect)
        {
            if (isCorrect)
                CorrectAlphagramCount++;
            else
                IncorrectAlphagramCount++;
        }
        public int[] GetBooleanAnswersThisQuestion()
        {
            int[] correct = { 0, 0 };
            foreach (var kvp in CurrentQuizAnswerStatsList)
            {
                if (kvp.IsCorrect)
                    correct[0]++;
                else
                    correct[1]++;
            }
            return correct;
        }


    }

    [Serializable]
    public class QuizAnswer
    {
        public string Word { get; set; }
        public bool IsCorrect { get; set; }

        public QuizAnswer(string word, bool isCorrect)
        {
            Word = word;
            IsCorrect = isCorrect;
        }
    }
}