using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Infrastructure;

namespace Pipeline.UnitTests
{
    [TestClass]
    public class IListDecimalExtensionsTests
    {
        /// <summary>
        /// http://statistics.about.com/od/Descriptive-Statistics/a/How-Do-We-Determine-What-Is-An-Outlier.htm
        /// </summary>
        [TestMethod]
        public void ExpectCorrectInterquartileRange()
        {
            var numbers = new[] { 1M, 2M, 2M, 3M, 3M, 4M, 5M, 5M, 9M }.ToList();
            var result = numbers.InterquartileWeakOutlierRange();
            Assert.AreEqual(-2.5M, result.Item1);
            Assert.AreEqual(9.5M, result.Item2);
        }
    }
}