using Pipeline;
using Pipeline.Extraction;

namespace Processor
{
    class Program
    {
        static void Main(string[] args)
        {
            var products = new IngestProductFile().Ingest("../../../Resources/products.txt");
            var listings = new IngestListingFile().Ingest("../../../Resources/listings.txt", 500);

            var simple = new SimplePipeline().FindMatches(products, listings);
        }
    }
}
