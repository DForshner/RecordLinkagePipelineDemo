using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Infrastructure;

namespace Pipeline.UnitTests.Infrastructure
{
    [TestClass]
    public class IEnumerableIntExtensionsTests
    {
        /// <summary>
        /// Test data from: https://rosettacode.org/wiki/Entropy#C.23
        /// </summary>
        [TestMethod]
        public void WhenCalculateEntropy_ExpectCorrect()
        {
            // "1223334444"
            var numFreq = new Dictionary<float, int>
            {
                { 1F, 1 },
                { 2F, 2 },
                { 3F, 3 },
                { 4F, 4 }
            };
            var totalNums = numFreq.Values.Sum();
            var h = numFreq.Select(x => x.Value).CalculateEntropy(totalNums);
            Assert.AreEqual(1.8464, h, 0.001);
        }

        [TestMethod]
        public void WhenAllSameCharacter_ExpectNoEntropy()
        {
            var charFreq = new Dictionary<string, int>
            {
                { "b", 10 },
            };
            var totalChars = charFreq.Values.Sum();

            var result = new[] { charFreq["b"] }.CalculateEntropy(totalChars);

            Assert.AreEqual(0F, result);
        }

        [TestMethod]
        public void WhenDiffFreq_ExpectHigherProbabilityHasMoreEntropy()
        {
            var charFreq = new Dictionary<string, int>
            {
                { "a", 2 },
                { "b", 4 },
                { "c", 8 },
                { "d", 10 }
            };
            var totalChars = charFreq.Values.Sum();

            var hOfb = new[] { charFreq["b"] }.CalculateEntropy(totalChars);
            var hOfC = new[] { charFreq["c"] }.CalculateEntropy(totalChars);
            Assert.IsTrue(hOfC > hOfb);
        }
    }
}