using System;
using System.Diagnostics;
using System.Linq;
using Pipeline.Shared;

namespace Pipeline.Extraction
{
    static class ProductParser
    {
        public static Product Parse(String str)
        {
            Debug.Assert(!String.IsNullOrEmpty(str));

            var fieldsAndValues = str
                .Remove(str.Length - 1, 1) // Remove end bracket }
                .Remove(0, 1) // Remove open bracket {
                .Split(',', ':')
                .Select(x => x
                    .ToUpperInvariant()
                    .Trim()
                    .Remove(x.Length - 1, 1) // Remove end quote
                    .Remove(0, 1) // Remove start quote
                )
                .ToList();

            return new Product
            {
                Name = fieldsAndValues[1],
                Manufacturer = fieldsAndValues[3],
                Model = fieldsAndValues[5],
                Family = fieldsAndValues[7]
                //AnnouncedDate = DateTime.Parse(fieldsAndValues[9])
            };
        }
    }
}
