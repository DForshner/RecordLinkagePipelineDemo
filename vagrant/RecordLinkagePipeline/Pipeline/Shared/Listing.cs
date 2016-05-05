using System;

namespace Pipeline.Shared
{
    public class Listing
    {
        public string Manufacturer { get; set; }

        public string Title { get; set; }

        public string CurrencyCode { get; set; }

        public Decimal Price { get; set; }

        public string Original { get; set; }

        public override string ToString()
        {
            return "{"
                + "Manufacturer: " + Manufacturer + ","
                + "Title: " + Title + ","
                + "CurrencyCode: " + CurrencyCode + ","
                + "Price: " + Price.ToString() + ","
                + "}";
        }
    }
}