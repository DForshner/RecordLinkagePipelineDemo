using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pipeline;
using Pipeline.Shared;
using Processor.IO;

namespace Processor
{
    public class Program
    {
        static ConcurrentQueue<string> _linesToLog = new ConcurrentQueue<string>();

        static void Main(string[] args)
        {
            // TODO: Mono's PWD is different that .Net's. Find a cleaner way of doing this.
            if (!IsMono())
            {
                Directory.SetCurrentDirectory("../../../");
            }

            var pipeline = CreatePipeline();

            // TODO: Get file locations from args

            // I'm passing these as functions to keep external I/O concerns out of the pipeline module. Could also use interface + constructor injection.
            Func<IEnumerable<string>> products = () =>  new FileReader().ReadLines("./Resources/products.txt");
            Func<IEnumerable<string>> listings = () =>  new FileReader().ReadLines("./Resources/listings.txt");
            Func<IEnumerable<string>> erates = () =>  new FileReader().ReadLines("./Resources/exchangeRates.txt");
            var matches = pipeline.FindMatches(products, listings, erates);

            System.IO.File.WriteAllLines(@"./log.txt", _linesToLog.Select(x => x));
            System.IO.File.WriteAllLines(@"./results.txt", ToJSON(matches));
        }

        private static IEnumerable<string> ToJSON(IEnumerable<ProductMatch> matches)
        {
            foreach (var match in matches.Where(x => x.Listings.Any()))
            {
                yield return match.Product.Original;
                foreach (var listing in match.Listings)
                {
                    yield return "\t" + listing.Original;
                }
            }
        }

        /// <summary>
        /// Wire up dependencies/composition root
        /// </summary>
        private static ListingsToProductResolutionPipeline CreatePipeline(bool fallbackAliasGenerator = false, bool fallbackAccessoryPruner = true, bool logToFile = false)
        {
            Action<string> log = x => _linesToLog.Enqueue(x);
            return new ListingsToProductResolutionPipeline(log);
        }

        public static bool IsMono()
        {
            return (Type.GetType("Mono.Runtime") != null);
        }
    }
}