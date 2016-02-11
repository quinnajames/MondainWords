using Microsoft.VisualStudio.TestTools.UnitTesting;
using MondainDeploy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MondainDeploy.Tests
{
    [TestClass()]
    public class QuizProcessTests
    {
        //todo: Add ProcessQuestion test
        //[TestMethod()]
        //public void ProcessQuestionTest()
        //{
        //    Assert.Fail();
        //}

        //todo: Add UpdateTotalSolutionsLabelWhenCorrect test
        //[TestMethod()]
        //public void UpdateTotalSolutionsLabelWhenCorrectTest()
        //{
        //    Assert.Fail();
        //}

        //todo: Add UpdateStats test
        [TestMethod()]
        public void UpdateStatsTest()
        {

            var mockAlphaToWords = new List<KeyValuePair<string, List<string>>>()
            {
                new KeyValuePair<string, List<string>>("AB", new List<string>() {"AB", "BA"}),
                new KeyValuePair<string, List<string>>("EFHRRTU", new List<string>() {"FURTHER"}),
                new KeyValuePair<string, List<string>>("AENORS", new List<string>() {"ARSENO", "REASON", "SENORA"}),
            };
            var mockQuiz = new Quiz(3, mockAlphaToWords, false);
            mockQuiz.SetWordAsCorrect("AB");
            mockQuiz.CurrentAnswerList.Remove("AB");
            mockQuiz.IncrementCounts(false);
            string [] expected = new string[4];
            expected[0] = "0/1";
            //expected[1] = "";
            //expected[2] = "";
            //expected[3] = "";
            Assert.AreEqual(expected[0], QuizProcess.UpdateStats(ref mockQuiz)["labelStatsCorrectAlphagramFraction"]);
                
        }
    }
}