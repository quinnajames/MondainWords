using System.Collections.Generic;
using System.Linq;
using System.Web.SessionState;

namespace MondainDeploy
{
    public struct Constants
    {
        public static readonly List<int> WordLengths = Enumerable.Range(2, 14).ToList();
        public static readonly int WordLengthMinDefault = 7;
        public static readonly int WordLengthMaxDefault = 8;
        public static readonly int DefaultQuizLength = 10;
        public const int ProbabilityMinDefault = 1;
        public const int ProbabilityMaxDefault = 99999;
        public static readonly string AnswerSetDefaultText = "Answers displayed here";
        public static readonly string QuizAlreadyFinishedText = "Error: Quiz is already completed!";
    }
}
