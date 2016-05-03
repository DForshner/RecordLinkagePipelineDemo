using System;
using System.IO;
using System.Linq;
using Pipeline;
using Pipeline.Analysis;
using Pipeline.Extraction;
using Pipeline.Pruning;

namespace Processor
{
    public class Program
    {
        static void Main(string[] args)
        {
            // TODO: Mono's PWD is different that .Net's. Find a cleaner way of doing this.
            if (!IsMono())
            {
                Directory.SetCurrentDirectory("../../../");
            }

            // TODO: Get file locations from args
            var products = new IngestProductFile().Ingest("./Resources/products.txt").ToList();
            var listings = new IngestListingFile().Ingest("./Resources/listings.txt", 1000).ToList();

            var pipeline = CreatedWiredUpPipeline();
            var matches = pipeline.FindMatches(products, listings);

            foreach(var match in matches)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("------------------------------------------------ {0} {1}", match.Product.Manufacturer, match.Product.Model);

                Console.ForegroundColor = ConsoleColor.White;
                foreach(var listing in match.Listings)
                {
                    Console.WriteLine(listing.Manufacturer);
                    Console.WriteLine(listing.Title);
                }
            }
        }

        /// <summary>
        /// Wire up pipeline dependencies (aka. composition root)
        /// </summary>
        private static SimplePipeline CreatedWiredUpPipeline(bool fallbackAliasGenerator = false, bool fallbackAccessoryPruner = false, bool logToFile = false)
        {
            var logger = (logToFile)
                // TODO: Log to file
                ? (Action<string>)((string x) => { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(x); Console.ForegroundColor = ConsoleColor.White; })
                : (Action<string>)((string x) => { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(x); Console.ForegroundColor = ConsoleColor.White; });

            IManufacturerNameAliasGenerator aliasGenerator = (fallbackAliasGenerator)
                ? (IManufacturerNameAliasGenerator)new DeterministicAliasGenerator() : (IManufacturerNameAliasGenerator)new SimilarityAliasGenerator();
            IListingPruner accessoryPruner = (fallbackAccessoryPruner)
                ? (IListingPruner)new DeterministicAccessoryPruner() : (IListingPruner)new TermUniquenessDistributionPruner();

            return new SimplePipeline(logger, aliasGenerator, accessoryPruner);
        }

        public static bool IsMono()
        {
            return (Type.GetType("Mono.Runtime") != null);
        }
    }
}