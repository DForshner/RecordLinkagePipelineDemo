using System;
using System.Diagnostics;
using System.Linq;
using Pipeline.Shared;

// TODO: Move to data access layer

namespace Pipeline.Extraction
{
    static class ListingParser
    {
        public static Listing Parse(String str)
        {
            Debug.Assert(!String.IsNullOrEmpty(str));

            var fieldsAndValues = str
                .Remove(str.Length - 1, 1) // Remove end bracket }
                .Remove(0, 1) // Remove open bracket {
                .Split(',', ':')
                .Select(x => x
                    .ToUpperInvariant()
                    .Trim(Environment.NewLine.ToCharArray())
                    .Remove(x.Length - 1, 1) // Remove end quote
                    .Remove(0, 1) // Remove start quote
                )
                .ToList();

            return new Listing
            {
                Title = fieldsAndValues[1],
                Manufacturer = fieldsAndValues[3],
                CurrencyCode = fieldsAndValues[5],
                Price = Decimal.Parse(fieldsAndValues[7])
            };
        }
    }
}
