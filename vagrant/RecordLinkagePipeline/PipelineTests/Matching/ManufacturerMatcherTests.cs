using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline;
using Pipeline.Matching;
using Pipeline.Shared;

namespace Pipeline.UnitTests.Matching
{
    [TestClass]
    public class ManufacturerMatcherTests
    {
        [TestMethod]
        public void ShouldMatchRepeatManufacturerNamesToOneCanonicalName()
        {
            var listings = new[]
            {
                new Listing { Manufacturer = "A A A" }
            };
            var manufacturers = new[] { "A", "B", "C" };
            var result = new ManufacturerListingsBlockGrouper().Match(listings, manufacturers);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("A", result.Single().ManufacturerName);
        }
    }
}