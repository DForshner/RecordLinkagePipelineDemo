using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Infrastructure;

namespace Pipeline.UnitTests.Infrastructure
{
    [TestClass]
    public class IListDoubleExtensionsTests
    {
        [TestMethod]
        public void ExpectCenterOfMassFound()
        {
            var testCases = new[]
            {
                new { Values = Enumerable.Empty<double>().ToList(), Expected = 0D },
                new { Values = new[] { 1D }.ToList(), Expected = 0D },
                new { Values = new[] { 5D, 5D }.ToList(), Expected = 0.5D },
                new { Values = new[] { 5D, 0D, 5D }.ToList(), Expected = 1D },
                new { Values = new[] { 1D, 0D, 0D, 0D, 1D }.ToList(), Expected = 2D },
                new { Values = new[] { 2D, 2D, 0D, 0D, 2D }.ToList(), Expected = 1.6D },
                new { Values = new[] { 2D, 0D, 0D, 2D, 2D }.ToList(), Expected = 2.3D }
            };
            foreach(var testCase in testCases)
            {
                var result = testCase.Values.CenterOfMassIndex();
                Assert.AreEqual(testCase.Expected, result, 0.1);
            }
        }
    }
}