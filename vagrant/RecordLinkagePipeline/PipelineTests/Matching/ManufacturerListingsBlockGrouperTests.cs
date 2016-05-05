using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Matching;
using Pipeline.Shared;

namespace Pipeline.UnitTests.Matching
{
    [TestClass]
    public class ManufacturerListingsBlockGrouperTests
    {
        [TestMethod]
        public void WhenListingManufacturerContainsCanonical_ExpectMatchedToCanonical()
        {
            var listings = new[]
            {
                new Listing { Manufacturer = "A A A" }
            };
            var canonical = new[] { "A", "B", "C" };
            var result = GetSut(canonical).Match(listings).Item1;
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("A", result.Single().ManufacturerName);
        }

        [TestMethod]
        public void WhenListingUsesAlias_ExpectMatchedToCanonical()
        {
            var listings = new[]
            {
                new Listing { Manufacturer = "Alias" }
            };
            var canonical = new[] { "Real", "Foo", "Bar" };
            var aliases = new[] { new ManufacturerNameAlias { Canonical = "Real", Alias = "Alias" } };
            var result = GetSut(canonical, aliases).Match(listings).Item1;
            Assert.AreEqual("Real", result.Single().ManufacturerName);
        }

        /// <summary>
        /// Bug found while testing
        /// </summary>
        [TestMethod]
        public void WhenCanonicalNameHasMultipleWords_ExpectMatch()
        {
            var listings = new[]
            {
                new Listing { Manufacturer = "konica minolta" }
            };
            var canonical = new[] { "konica minolta" };
            var result = GetSut(canonical).Match(listings).Item1.Single();
            Assert.IsNotNull(result);
        }

        private ManufacturerListingsBlockGrouper GetSut(IEnumerable<string> canonical = null, IEnumerable<ManufacturerNameAlias> aliases = null)
        {
            canonical = canonical ?? Enumerable.Empty<string>();
            aliases = aliases ?? Enumerable.Empty<ManufacturerNameAlias>();

            return new ManufacturerListingsBlockGrouper(canonical, aliases);
        }
    }
}