using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Infrastructure;

namespace Pipeline.UnitTests.Infrastructure
{
    [TestClass]
    public class FieldMungerTests
    {
        [TestMethod]
        public void WhenAllValid_ExpectNoChange()
        {
            var expected = FieldMunger.Munge("1 This is a test 1");
            var result = FieldMunger.Munge("1 This is a test 1");
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ExpectNonAlphaOrNumericCharRemoved()
        {
            var expected = FieldMunger.Munge("1 This is a test 1 ");
            var result = FieldMunger.Munge("1. This=is-a test 1!!!");
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Fixes bug found during testing
        /// </summary>
        [TestMethod]
        public void WhenFieldHasNoValidCharactersOrNumbers_ExpectEmptyString()
        {
            var result = FieldMunger.Munge("----------------------------");
            Assert.AreEqual("", result);
        }
    }
}