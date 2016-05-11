using System;
using System.Collections.Generic;
using System.Diagnostics;
using Pipeline.Infrastructure;
using Pipeline.Shared;
using System.Linq;
using Pipeline.Domain;

namespace Pipeline.Output
{
    internal static class ProductMatchDtoMapper
    {
        public static IEnumerable<ProductMatchDto> Map(IEnumerable<ProductMatch> matches)
        {
            Debug.Assert(matches != null, "Expected matches not null.");
            foreach(var match in matches)
            {
                var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(match.Product.Original);
                var productName = (string)jsonObj["product_name"];

                var listings = match.Listings
                    .Select(x => x.Original)
                    .Select(ListingDtoParser.Parse)
                    .ToList();

                yield return new ProductMatchDto
                {
                    Name = productName,
                    Listings = listings
                };
            }
        }
    }
}
