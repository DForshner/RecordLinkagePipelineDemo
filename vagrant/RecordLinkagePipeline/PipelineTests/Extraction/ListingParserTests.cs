﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline;
using Pipeline.Extraction;

namespace Pipeline.UnitTests.Extraction
{
    [TestClass]
    public class ListingParserTests
    {
        [TestMethod]
        public void ExpectParsesWellFormedRecordsIntoListings()
        {
            var src = @"{""title"":""Canon PowerShot ELPH 300 HS (Black)"",""manufacturer"":""Canon Canada"",""currency"":""CAD"",""price"":""259.99""}";
            var result = ListingParser.Parse(src);
            Assert.AreEqual("Canon PowerShot ELPH 300 HS Black".ToLowerInvariant(), result.Title);
            Assert.AreEqual("Canon Canada".ToLowerInvariant(), result.Manufacturer);
            Assert.AreEqual("CAD".ToLowerInvariant(), result.CurrencyCode);
            Assert.AreEqual(259.99M, result.Price);
        }
    }
}