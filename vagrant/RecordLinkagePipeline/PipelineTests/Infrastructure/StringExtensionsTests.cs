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
            var results = "abcd".CreateNGrams(2, 4);
            Assert.IsTrue(results.OrderBy(x => x).AsQueryable().SequenceEqual(new[] { "ab", "abc", "abcd", "bc", "bcd", "cd" }.AsQueryable()));
        }

        [TestMethod]
        public void CreateBiTriTokenShingles_WhenSingleToken_ExpectNoResults()
        {
            var results = "a".CreateNGrams(2, 3);
            Assert.IsTrue(!results.Any());
        }
    }
}