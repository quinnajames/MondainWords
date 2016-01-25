using Microsoft.VisualStudio.TestTools.UnitTesting;
using MondainDeploy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MondainDeploy.Tests
{
    [TestClass()]
    public class MondainUITests
    {
        [TestMethod()]
        public void TryParseWithDefaultTest()
        {
            string original = "2";
            int expected = 2;
            Assert.AreEqual(expected, MondainUI.TryParseWithDefault(original, 0));
        }

        public void TryParseWithDefaultTest1()
        {
            string original = "cat_🐱";
            int expected = 0;
            Assert.AreEqual(expected, MondainUI.TryParseWithDefault(original, 0));
        }

        public void TryParseWithDefaultTest2()
        {
            string original = "4.5";
            int expected = 4;
            Assert.AreEqual(expected, MondainUI.TryParseWithDefault(original, 0));
        }

        public void TryParseWithDefaultTest3()
        {
            string original = "-5.5";
            int expected = -6;
            Assert.AreEqual(expected, MondainUI.TryParseWithDefault(original, 0));
        }

        [TestMethod()]
        public void ItalicizeTest()
        {
            string original = "test";
            string expected = "<i>test</i>";
            Assert.AreEqual(expected, MondainUI.Italicize(original));
        }

        [TestMethod()]
        public void EmboldenTest()
        {
            string original = "test";
            string expected = "<strong>test</strong>";
            Assert.AreEqual(expected, MondainUI.Embolden(original));
        }

        [TestMethod()]
        public void StrikeTest()
        {
            string original = "test";
            string expected = "<span style=\"color:lightgray\"><del>test</del></span>";
            Assert.AreEqual(expected, MondainUI.Strike(original));
        }

        //[TestMethod()]
        //public void PrependStringTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void PrependLineTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void PostpendLineTest()
        //{
        //    Assert.Fail();
        //}
    }
}