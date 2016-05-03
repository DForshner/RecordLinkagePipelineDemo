using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline;
using Pipeline.Extraction;

namespace Pipeline.UnitTests.Extraction
{
    [TestClass]
    public class MungerTests
    {
        [TestMethod]
        public void WhenAllValid_ExpectNoChange()
        {
            var expected = Munger.Munge("1 This is a test 1");
            var result = Munger.Munge("1 This is a test 1");
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ExpectNonAlphaOrNumericCharRemoved()
        {
            var expected = Munger.Munge("1 This is a test 1 ");
            var result = Munger.Munge("1. This=is-a test 1!!!");
            Assert.AreEqual(expected, result);
        }
    }
}