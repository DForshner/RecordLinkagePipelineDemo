using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Pipeline;
using Pipeline.Shared;
using Processor.IO;

namespace Processor
{
    public class Program
    {
        private static ConcurrentQueue<string> _linesToLog = new ConcurrentQueue<string>();

        /// <summary>
        /// Main entry point
        /// Args:
        ///     -- pretty-print for indented json output
        /// </summary>
        public static void Main(string[] args)
        {
            // KLUDGE: Mono's PWD is different that .Net's.
            // TODO: Find a cleaner way of doing this.
            if (!IsMono())
            {
                Directory.SetCurrentDirectory("../../../");
            }

            // TODO: Get file locations from args
            var reader = new FileReader();
            var pipeline = CreatePipeline(reader);
            var matches = FindMatches(reader, pipeline);

            var writer = new FileWriter();
            writer.WriteLines(@"./log.txt", _linesToLog.Select(x => x));

            WriteMatchesToFile(args, matches, writer);
        }

        private static void WriteMatchesToFile(string[] args, IEnumerable<ProductMatchDto> matches, FileWriter writer)
        {
            var outputFormatting = (args.Any() && args.Contains("--pretty-print")) ? Formatting.Indented : Formatting.None;
            var output = matches.Select(x => JsonConvert.SerializeObject(x, outputFormatting));
            writer.WriteLines(@"./results.txt", output);
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

        private static IEnumerable<ProductMatchDto> FindMatches(FileReader reader, ListingsToProductResolutionPipeline pipeline)
        {
            var products = reader.ReadLines("./Resources/products.txt");
            var listings = reader.ReadLines("./Resources/listings.txt");
            return pipeline.FindMatches(products, listings);
        }
    }
}