using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline;
using Pipeline.Extraction;

namespace Pipeline.UnitTests.Extraction
{
    [TestClass]
    public class ExchangeRateParserTests
    {
        [TestMethod]
        public void WhenWellFormed_ExpectParsed()
        {
            var src = @"{ ""source"": ""eur"", ""destination"": ""cad"", ""rate"": 1.4 }";
            var result = ExchangeRateParser.Parse(src);
            Assert.AreEqual("eur", result.SourceCurrencyCode);
            Assert.AreEqual("cad", result.DestinationCurrencyCode);
            Assert.AreEqual(1.4M, result.Rate);
        }
    }
}