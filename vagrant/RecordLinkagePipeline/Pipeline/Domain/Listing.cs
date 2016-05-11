using System;

namespace Pipeline.Domain
{
    internal class Listing
    {
        /// <summary>
        /// Who manufactures the product for sale
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Description of product for sale
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Currency code, e.g. USD, CAD, GBP, etc.
        /// </summary>
        public string CurrencyCode { get; set; }

        public Decimal Price { get; set; }

        public string Original { get; set; }
    }
}