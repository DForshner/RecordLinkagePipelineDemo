using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Matching;
using Pipeline.Shared;

namespace Pipeline.UnitTests.Matching
{
    [TestClass]
    public class ProductModelMatcherTests
    {
        [TestMethod]
        public void WhenProductModelMatchesListingTitle_ExpectMatched()
        {
            var listings = new ManufacturerNameListingsBlock
            {
                Listings = new[]
                {
                    new Listing { Title = "ModelA" }
                }
            };
            var products = new ManufacturerNameProductsBlock
            {
                Products = new[]
                {
                    new Product { Model = "ModelA" },
                    new Product { Model = "ModelB" },
                }
            };
            var result = GetSut().FindProductMatchs(listings, products).Item1.Single();
            Assert.AreEqual("ModelA", result.Product.Model);
            Assert.AreEqual("ModelA", result.Listings.Single().Title);
        }

        private ProductModelMatcher GetSut()
        {
            return new ProductModelMatcher();
        }
    }
}