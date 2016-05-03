using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Pruning;
using Pipeline.Shared;

namespace Pipeline.UnitTests.Pruning
{
    [TestClass]
    public class ProductMatchCostOutlierPrunerTests
    {
        [TestMethod]
        public void WhenPricesSimilar_ExpectNonePruned()
        {
            var ratesBySrc = new[]
            {
                new ExchangeRate { SourceCurrencyCode = "CAD", DestinationCurrencyCode = "CAD", Rate = 1.0M },
            };
            var matches = new[]
            {
                new ProductMatch(new Product(), new []
                {
                    new Listing { Price = 90M, CurrencyCode = "CAD" },
                    new Listing { Price = 100M, CurrencyCode = "CAD" },
                    new Listing { Price = 110M, CurrencyCode = "CAD" },
                })
            };

            var results = new ProductMatchCostOutlierPruner(ratesBySrc).Prune(matches)
                .Single()
                .Listings;
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
            var matches = new[]
            {
                new ProductMatch(new Product(), new []
                {
                    new Listing { Price = 90M, CurrencyCode = "CAD" },
                    new Listing { Price = 100M, CurrencyCode = "CAD" },
                    new Listing { Price = 100M, CurrencyCode = "CAD" },
                    new Listing { Price = 100M, CurrencyCode = "CAD" },
                    new Listing { Price = 100M, CurrencyCode = "CAD" },
                    new Listing { Price = 100M, CurrencyCode = "CAD" },
                    new Listing { Price = 110M, CurrencyCode = "CAD" },

                    new Listing { Price = 2000M, CurrencyCode = "CAD" },
                })
            };

            var results = new ProductMatchCostOutlierPruner(ratesBySrc).Prune(matches)
                .Single()
                .Listings
                .Select(x => x.Price);

            Assert.AreEqual(7, results.Count());
            Assert.IsTrue(results.All(x => x <= 110M));
        }
    }
}