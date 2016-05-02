using System;
using System.IO;
using Pipeline;
using Pipeline.Extraction;

namespace Processor
{
    public class Program
    {
        static void Main(string[] args)
        {
            // TODO: Find some cleaner way of doing this.
            if (!IsMono())
            {
                Directory.SetCurrentDirectory("../../../");
            }

            var products = new IngestProductFile().Ingest("./Resources/products.txt");
            var listings = new IngestListingFile().Ingest("./Resources/listings.txt", 500);

            Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            Console.WriteLine(Environment.CurrentDirectory);

            foreach(var prod in products)
            {
                Console.WriteLine("{0} {1}", prod.Manufacturer, prod.Model);
            }
            foreach(var listing in listings)
            {
                Console.WriteLine("{0} {1}", listing.Manufacturer, listing.Title);
            }
        }

        public static bool IsMono()
        {
            return (Type.GetType("Mono.Runtime") != null);
        }
    }
}
