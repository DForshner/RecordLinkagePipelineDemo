using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline;
using Pipeline.Extraction;

namespace Pipeline.UnitTests.Extraction
{
    [TestClass]
    public class ProductParserTests
    {
        [TestMethod]
        public void ExpectParsesWellFormedRecordsIntoProducts()
        {
            var src = @"{""product_name"":""Toshiba_PDR-M60"",""manufacturer"":""Toshiba"",""model"":""PDR-M60"",""announced-date"":""2000-02-02T19:00:00.000-05:00""}";
            var result = ProductParser.Parse(src);
            Assert.AreEqual("Toshiba PDR M60".ToLowerInvariant(), result.Name);
            Assert.AreEqual("Toshiba".ToLowerInvariant(), result.Manufacturer);
            Assert.AreEqual("PDR M60".ToLowerInvariant(), result.Model);
        }

        [TestMethod]
        public void WhenFamily_ExpectParses()
        {
            var src = @"{""product_name"":""Canon_IXUS_105"",""manufacturer"":""Canon"",""model"":""105"",""family"":""IXUS"",""announced-date"":""2010-02-07T19:00:00.000-05:00""}";
            var result = ProductParser.Parse(src);
            Assert.AreEqual("ixus".ToLowerInvariant(), result.Family);
        }
    }
}
