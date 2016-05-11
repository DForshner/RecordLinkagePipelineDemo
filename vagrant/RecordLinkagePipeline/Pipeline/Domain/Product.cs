
namespace Pipeline.Domain
{
    internal class Product
    {
        /// <summary>
        /// A unique id for the product
        /// </summary>
        public string Name { get; set; }

        public string Manufacturer { get; set; }

        public string Model { get; set; }

        /// <summary>
        /// Optional grouping of products
        /// </summary>
        public string Family { get; set; }

        public string Original { get; set; }
    }
}
