﻿using System;
using System.Diagnostics;
using Pipeline.Domain;
using Pipeline.Infrastructure;
using Pipeline.Shared;

namespace Pipeline.Extraction
{
    internal static class ListingParser
    {
        public static Listing Parse(String str)
        {
            Debug.Assert(!String.IsNullOrEmpty(str));

            var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(str);
            return new Listing
            {
                Title = FieldMunger.Munge((string)jsonObj["title"]),
                Manufacturer = FieldMunger.Munge((string)jsonObj["manufacturer"]),
                Price = Decimal.Parse((string)jsonObj["price"]),
                CurrencyCode = FieldMunger.Munge((string)jsonObj["currency"]),
                Original = str
            };
        }
    }
}
