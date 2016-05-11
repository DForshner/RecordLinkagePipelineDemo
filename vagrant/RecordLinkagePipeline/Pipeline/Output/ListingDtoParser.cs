using System;
using System.Diagnostics;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Output
{
    internal static class ListingDtoParser
    {
        public static ListingDto Parse(String str)
        {
            Debug.Assert(!String.IsNullOrEmpty(str));

            var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(str);
            return new ListingDto
            {
                Manufacturer = (string)jsonObj["manufacturer"],
                Price = Decimal.Parse((string)jsonObj["price"]),
                CurrencyCode = (string)jsonObj["currency"],
                Title = (string)jsonObj["title"],
            };
        }
    }
}
