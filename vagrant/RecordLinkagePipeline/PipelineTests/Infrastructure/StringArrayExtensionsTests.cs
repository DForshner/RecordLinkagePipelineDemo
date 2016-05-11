using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Infrastructure;

namespace Pipeline.UnitTests
{
    [TestClass]
    public class StringArrayExtensionsTests
    {
        [TestMethod]
        public void CreateNGrams_WhenFourChars_ExpectSingleQuadGram()
        {
            var results = new[] { "a", "b", "c", "d" }.CreateNShingles(4);
            Assert.AreEqual("abcd", results.Single());
        }

        [TestMethod]
        public void CreateNGrams_WhenFourChars_ExpectAll()
        {
            var results = new[] { "a", "b", "c", "d" }.CreateNShingles(1, 4);
            Assert.IsTrue(results.OrderBy(x => x).AsQueryable().SequenceEqual(new[] { "a", "ab", "abc", "abcd", "b", "bc", "bcd", "c", "cd", "d" }.AsQueryable()));
        }

        [TestMethod]
        public void CreateUniBiTokenShingles_WhenThreeTokens_ExpectUniBiShingles()
        {
            var results = new[] { "ant", "bee", "cat" }.CreateNShingles(1, 2);
            Assert.IsTrue(results.OrderBy(x => x).AsQueryable().SequenceEqual(new[] { "ant", "antbee", "bee", "beecat", "cat" }.AsQueryable()));
        }

        [TestMethod]
        public void CreateBiTriTokenShingles_WhenSingleToken_ExpectNoResults()
        {
            var results = new[] { "ant" }.CreateNShingles(2, 3);
            Assert.IsTrue(!results.Any());
        }

        [TestMethod]
        public void CreateBiTriTokenShingles_WhenTwoTokens_ExpectOneBiShingle()
        {
            var results = new[] { "ant", "bee" }.CreateNShingles(2, 3);
            Assert.AreEqual("antbee", results.Single());
        }
    }
}