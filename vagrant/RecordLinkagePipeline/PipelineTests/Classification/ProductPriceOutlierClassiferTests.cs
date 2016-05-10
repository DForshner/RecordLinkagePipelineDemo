using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Classification;
using Pipeline.Shared;

namespace Pipeline.UnitTests.Classification
{
    [TestClass]
    public class ProductPriceOutlierClassiferTests
    {
        [TestMethod]
        public void WhenPricesSimilar_ExpectNonePruned()
        {
            var ratesBySrc = new[]
            {
                new ExchangeRate { SourceCurrencyCode = "CAD", DestinationCurrencyCode = "CAD", Rate = 1.0M },
            };
            var matches = new ProductMatch(new Product(), new[]
            {
                new Listing { Price = 90M, CurrencyCode = "CAD" },
                new Listing { Price = 100M, CurrencyCode = "CAD" },
                new Listing { Price = 110M, CurrencyCode = "CAD" },
            });

            var results = CreateSut(ratesBySrc).ClassifyAsCamera(matches);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public void WhenOneListingHigherThanOthers_ExpectFiltered()
        {
            var ratesBySrc = new[]
            {
                new ExchangeRate { SourceCurrencyCode = "CAD", DestinationCurrencyCode = "CAD", Rate = 1.0M },
                new ExchangeRate { SourceCurrencyCode = "USD", DestinationCurrencyCode = "CAD", Rate = 2.0M },
                new ExchangeRate { SourceCurrencyCode = "EUR", DestinationCurrencyCode = "CAD", Rate = 4.0M }
            };
            var matches = new ProductMatch(new Product(), new []
            {
                new Listing { Price = 90M, CurrencyCode = "CAD" },
                new Listing { Price = 100M, CurrencyCode = "CAD" },
                new Listing { Price = 100M, CurrencyCode = "CAD" },
                new Listing { Price = 100M, CurrencyCode = "CAD" },
                new Listing { Price = 100M, CurrencyCode = "CAD" },
                new Listing { Price = 100M, CurrencyCode = "CAD" },
                new Listing { Price = 110M, CurrencyCode = "CAD" },

                new Listing { Price = 2000M, CurrencyCode = "CAD" },
            });

            var results = CreateSut(ratesBySrc).ClassifyAsCamera(matches)
                .Where(x => x.Item2)
                .Select(x => x.Item1.Price);

            Assert.AreEqual(7, results.Count());
            Assert.IsTrue(results.All(x => x <= 110M));
        }

        [TestMethod]
        public void WhenPricesSimilar_ExpectAllClassifiedAsCamera()
        {
            var prices = new[] { 306M, 420M, 365M, 386M, 451M, 515M };
            var matches = new ProductMatch(new Product(), prices.Select(x => new Listing { Price = x, CurrencyCode = "" }).ToList());

            var results = CreateSut(Enumerable.Empty<ExchangeRate>()).ClassifyAsCamera(matches)
                .Where(x => x.Item2)
                .Select(x => x.Item1.Price);

            Assert.AreEqual(6, results.Count());
        }

        private static ProductPriceOutlierClassifer CreateSut(IEnumerable<ExchangeRate> ratesBySrc)
        {
            return new ProductPriceOutlierClassifer(ratesBySrc, 0.5M, 5M);
        }
    }
}