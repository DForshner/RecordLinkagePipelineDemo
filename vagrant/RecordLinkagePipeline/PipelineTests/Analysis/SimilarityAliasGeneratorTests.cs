using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Analysis;
using Pipeline.Domain;
using Pipeline.Shared;

namespace Pipeline.UnitTests.Analysis
{
    [TestClass]
    public class SimilarityAliasGeneratorTests
    {
        [TestMethod]
        public void WhenSinglePossibleMatchWithSimilarManufacturerName_ExpectMatch()
        {
            var products = new[]
            {
                new Product { Manufacturer = "acme", Model = "model5" }
            };
            var listings = new[]
            {
                new Listing { Manufacturer = "acme corp", Title = "camera zoom model5" }
            };
            var termProbablities = TokenProbabilityCalculator.GetProbabilities(listings, x => x.Title);

            var result = CreateSut().Generate(products, listings, termProbablities).Single();

            Assert.AreEqual("acme", result.Canonical);
            Assert.AreEqual("acme corp", result.Alias);
        }

        [TestMethod]
        public void WhenMultiPossibleModelMatchs_ExpectClosestWins()
        {
            var products = new[]
            {
                new Product { Manufacturer = "acme", Model = "model5" }
            };
            var listings = new[]
            {
                new Listing { Manufacturer = "acme corp", Title = "camera zoom model5" },
                new Listing { Manufacturer = "acme inc", Title = "camera zoom dxc 5" },
                new Listing { Manufacturer = "acme ltd", Title = "camera zoom model3" }
            };
            var termProbablities = TokenProbabilityCalculator.GetProbabilities(listings, x => x.Title);

            var result = CreateSut().Generate(products, listings, termProbablities).Single();

            Assert.AreEqual("acme", result.Canonical);
            Assert.AreEqual("acme corp", result.Alias);
        }

        [TestMethod]
        public void WhenModelIsCommonListingTerm_ExpectNoMatch()
        {
            var products = new[]
            {
                new Product { Manufacturer = "acme", Model = "zoom" }
            };
            var listings = new[]
            {
                new Listing { Manufacturer = "acme corp", Title = "camera zoom 15x zoom" },
                new Listing { Manufacturer = "acme inc", Title = "camera zoom 20x zoom" },
                new Listing { Manufacturer = "acme ltd", Title = "camera zoom" }
            };
            var termProbablities = TokenProbabilityCalculator.GetProbabilities(listings, x => x.Title);

            var results = CreateSut().Generate(products, listings, termProbablities);

            Assert.IsFalse(results.Any());
        }

        private static SimilarityAliasGenerator CreateSut()
        {
            return new SimilarityAliasGenerator(33, 0.50F, 0.90F);
        }
    }
}