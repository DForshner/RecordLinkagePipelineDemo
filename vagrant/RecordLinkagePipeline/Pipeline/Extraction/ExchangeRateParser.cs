using System;
using System.Diagnostics;
using Pipeline.Domain;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Extraction
{
    internal static class ExchangeRateParser
    {
        public static ExchangeRate Parse(String str)
        {
            Debug.Assert(!String.IsNullOrEmpty(str));

            var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(str);
            return new ExchangeRate
            {
                SourceCurrencyCode = FieldMunger.Munge((string)jsonObj["source"]),
                DestinationCurrencyCode = FieldMunger.Munge((string)jsonObj["destination"]),
                Rate = Decimal.Parse((string)jsonObj["rate"])
            };
        }
    }
}
