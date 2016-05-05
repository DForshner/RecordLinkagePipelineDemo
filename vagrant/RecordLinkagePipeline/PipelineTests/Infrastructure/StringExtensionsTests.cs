using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Infrastructure;

namespace Pipeline.UnitTests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void ExpectBiTriQuadCharacterNGramsCreated()
        {
            var results = "abcd".CreateBiTriQuadCharacterNGrams();
            Assert.IsTrue(results.OrderBy(x => x).AsQueryable().SequenceEqual(new[] { "ab", "abc", "abcd", "bc", "bcd", "cd" }.AsQueryable()));
        }

        [TestMethod]
        public void CreateUniBiTokenShingles_WhenThreeTokens_ExpectUniBiShingles()
        {
            var results = "ant bee cat".CreateUniBiTokenShingles();
            Assert.IsTrue(results.OrderBy(x => x).AsQueryable().SequenceEqual(new[] { "ant", "antbee", "bee", "beecat", "cat" }.AsQueryable()));
        }

        [TestMethod]
        public void CreateBiTriTokenShingles_WhenSingleToken_ExpectNoResults()
        {
            var results = "ant".CreateBiTriTokenShingles();
            Assert.IsTrue(!results.Any());
        }

        [TestMethod]
        public void CreateBiTriTokenShingles_WhenTwoTokens_ExpectOneBiShingle()
        {
            var results = "ant bee".CreateBiTriTokenShingles();
            Assert.AreEqual("antbee", results.Single());
        }
    }
}