using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline;
using Pipeline.Analysis;

namespace Pipeline.UnitTests.Analysis
{
    [TestClass]
    public class WordFrequencyPerCollectionTests
    {
        private class Fake
        {
            public string Data { get; set; }
        }

        [TestMethod]
        public void ExpectCountsNumberOfDocsWordsAppearIn()
        {
            var docs = new[]
                {
                    new Fake { Data = "A B C D E F" },
                    new Fake { Data = "A B C D E F" },
                    new Fake { Data = "A B C D" },
                    new Fake { Data = "A B C D" },
                    new Fake { Data = "A B" },
                    new Fake { Data = "A B" },
                    new Fake { Data = "" },
                };
            var result = new WordFrequencyPerCollection().Count(docs, x => x.Data);
            Assert.AreEqual(Math.Log(7D / 6D), result["A"], 0.01D);
            Assert.AreEqual(Math.Log(7D / 2D), result["F"], 0.01D);
            Assert.IsFalse(result.ContainsKey("G"));
        }
    }
}