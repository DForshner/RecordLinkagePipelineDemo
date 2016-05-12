using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Pipeline;
using Pipeline.Shared;
using Processor.IO;
using Processor.Log;

namespace Processor
{
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// Args:
        ///     -- pretty-print for indented json output
        /// </summary>
        public static void Main(string[] args)
        {
            var logger = new Logger(@"./log.txt");
            logger.Log("Start");

            // KLUDGE: Mono's PWD is different that .Net's.
            // TODO: Find a cleaner way of doing this.
            if (!IsMono())
            {
                Directory.SetCurrentDirectory("../../../");
            }

            // TODO: Get file locations from args

            var reader = new FileReader();
            var pipeline = CreatePipeline(reader, logger);
            var matches = FindMatches(reader, pipeline);

            var isPrettyPrint = (args.Any() && args.Contains("--pretty-print"));
            logger.Log(String.Format("Writing results, Pretty Print {0}", isPrettyPrint));
            WriteMatchesToFile(isPrettyPrint, matches);

            logger.Log("End");
            logger.StopLogging();
        }

        private static bool IsMono()
        {
            return (Type.GetType("Mono.Runtime") != null);
        }

        private static ListingsToProductResolutionPipeline CreatePipeline(FileReader reader, Logger logger)
        {
            var config = String.Concat(reader.ReadLines("./Resources/config.txt"));
            var erates = reader.ReadLines("./Resources/exchangeRates.txt");
            var cameraTrainingSet = reader.ReadLines("./Resources/cameraTrainingSet.txt");
            var accessoryTrainingSet = reader.ReadLines("./Resources/accessoryTrainingSet.txt");
            return new ListingsToProductResolutionPipeline(logger.Log, config, erates, cameraTrainingSet, accessoryTrainingSet);
        }

        private static IEnumerable<ProductMatchDto> FindMatches(FileReader reader, ListingsToProductResolutionPipeline pipeline)
        {
            var products = reader.ReadLines("./Resources/products.txt");
            var listings = reader.ReadLines("./Resources/listings.txt");
            return pipeline.FindMatches(products, listings);
        }

        private static void WriteMatchesToFile(bool isPrettyPrint, IEnumerable<ProductMatchDto> matches)
        {
            var outputFormatting = isPrettyPrint ? Formatting.Indented : Formatting.None;
            var output = matches.Select(x => JsonConvert.SerializeObject(x, outputFormatting));

            var writer = new FileWriter();
            writer.WriteLines(@"./results.txt", output);
        }
    }
}