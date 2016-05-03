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
            Assert.AreEqual("Toshiba_PDR-M60".ToUpperInvariant(), result.Name);
            Assert.AreEqual("Toshiba".ToUpperInvariant(), result.Manufacturer);
            Assert.AreEqual("PDR-M60".ToUpperInvariant(), result.Model);
            //Assert.AreEqual(new DateTime(2000, 2, 2, 14, 0, 0, DateTimeKind.Utc), result.AnnouncedDate.ToUniversalTime());
        }
    }
}
