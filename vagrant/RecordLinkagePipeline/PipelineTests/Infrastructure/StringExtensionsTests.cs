using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Infrastructure;

namespace Pipeline.UnitTests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void ExpectBiTriQuadCharacterNGramsFromManufacturerName()
        {
            var results = "abcd".CreateBiTriQuadCharacterNGrams();
            Assert.IsTrue(results.OrderBy(x => x).AsQueryable().SequenceEqual(new[] { "ab", "abc", "abcd", "bc", "bcd", "cd" }.AsQueryable()));
        }

        [TestMethod]
        public void ExpectUniBiShinglesFromModelNames()
        {
            var results = "ant bee cat".CreateUniBiTokenShingles();
            Assert.IsTrue(results.OrderBy(x => x).AsQueryable().SequenceEqual(new[] { "ant", "antbee", "bee", "beecat", "cat" }.AsQueryable()));
        }
    }
}