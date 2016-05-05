using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Infrastructure;

namespace Pipeline.UnitTests.Infrastructure
{
    [TestClass]
    public class IListTExtensionsTests
    {
        [TestMethod]
        public void LastIndexWhere_WhenNoElements_ExpectNegativeOne()
        {
            var test = Enumerable.Empty<int>().ToList();
            var result = test.LastIndexWhere(x => x == 1);
            const int NOT_FOUND = -1;
            Assert.AreEqual(NOT_FOUND, result);
        }

        [TestMethod]
        public void LastIndexWhere_WhenNoMatchingElement_ExpectNegativeOne()
        {
            var test = new[] { 0, 1, 2, 1, 2, 3 };
            var result = test.LastIndexWhere(x => x == 6);
            const int NOT_FOUND = -1;
            Assert.AreEqual(NOT_FOUND, result);
        }

        [TestMethod]
        public void LastIndexWhere_WhenMultipleMatchingElements_ExpectLastOccuranceFound()
        {
            var test = new[] { 0, 1, 2, 1, 2, 3 };
            var result = test.LastIndexWhere(x => x == 3);
            Assert.AreEqual(5, result);
        }
    }
}