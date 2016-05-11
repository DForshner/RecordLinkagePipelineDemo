using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Analysis;
using Pipeline.Domain;
using Pipeline.Shared;

namespace Pipeline.UnitTests.Analysis
{
    [TestClass]
    public class TokenProbabilityCalculatorTests
    {
        [TestMethod]
        public void ExpectTokenProbablityPerDoc()
        {
            var docs = new[]
            {
                new Listing { Title = "A B C D E F" },
                new Listing { Title = "A B C D E F" },
                new Listing { Title = "A B C D" },
                new Listing { Title = "A B C D" },
                new Listing { Title = "A B" },
                new Listing { Title = "A B" },
                new Listing { Title = "" },
            };
            var result = TokenProbabilityCalculator.GetProbabilities(docs, x => x.Title);

            Assert.AreEqual(6F / 7F, result["A"], 0.01D);
            Assert.AreEqual(2F / 7F, result["F"], 0.01D);
            Assert.IsFalse(result.ContainsKey("G"));
        }
    }
}