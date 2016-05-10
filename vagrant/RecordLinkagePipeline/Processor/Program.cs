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
        private static ConcurrentQueue<string> _linesToLog = new ConcurrentQueue<string>();

        public static void Main(string[] args)
        {
            // TODO: Mono's PWD is different that .Net's. Find a cleaner way of doing this.
            if (!IsMono())
            {
                Directory.SetCurrentDirectory("../../../");
            }

            // TODO: Get file locations from args
            var reader = new FileReader();
            var pipeline = CreatePipeline(reader);
            var matches = FindMatches(reader, pipeline);

            WriteOutput(matches);
        }

        private static bool IsMono()
        {
            return (Type.GetType("Mono.Runtime") != null);
        }

        private static ListingsToProductResolutionPipeline CreatePipeline(FileReader reader)
        {
            Action<string> log = x => _linesToLog.Enqueue(x);
            var config = String.Concat(reader.ReadLines("./Resources/config.txt"));
            var erates = reader.ReadLines("./Resources/exchangeRates.txt");
            var cameraTrainingSet = reader.ReadLines("./Resources/cameraTrainingSet.txt");
            var accessoryTrainingSet = reader.ReadLines("./Resources/accessoryTrainingSet.txt");
            return new ListingsToProductResolutionPipeline(log, config, erates, cameraTrainingSet, accessoryTrainingSet);
        }

        private static IEnumerable<ProductMatch> FindMatches(FileReader reader, ListingsToProductResolutionPipeline pipeline)
        {
            var products = reader.ReadLines("./Resources/products.txt");
            var listings = reader.ReadLines("./Resources/listings.txt");
            return pipeline.FindMatches(products, listings);
        }

        private static void WriteOutput(IEnumerable<ProductMatch> matches)
        {
            var writer = new FileWriter();
            writer.WriteLines(@"./log.txt", _linesToLog.Select(x => x));
            writer.WriteLines(@"./results.txt", ToJSON(matches));
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
    }
}