using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Pipeline;
using Pipeline.Infrastructure;
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
        ///     -- pretty-print - for indented json output
        ///     -- config-file {filename} - location of configuration file
        ///     -- exchange-rate-file {filename} - location of exchange rate file
        ///     -- camera-training-set-file {filename} - location of camera training set file
        ///     -- accessory-training-set-file {filename} - location of accessory training set file
        ///     -- products-file {filename} - location of products file
        ///     -- listings-file {filename} - location of listings file
        /// Example: --pretty-print --config-file ./Resources/config.json --exchange-rate-file ./Resources/exchangeRates.json --camera-training-set-file ./Resources/cameraTrainingSet.txt --accessory-training-set-file ./Resources/accessoryTrainingSet.txt --products-file ./Resources/products.txt --listings-file ./Resources/listings.txt
        /// </summary>
        public static void Main(string[] args)
        {
            var logger = new Logger(@"./log.txt");
            logger.Log("Start");
            logger.Log("Args: " + String.Join(", ", args));

            // KLUDGE: Mono's PWD is different that .Net's.
            // TODO: Find a cleaner way of doing this.
            if (!IsMono())
            {
                Directory.SetCurrentDirectory("../../../");
            }

            try
            {
                var reader = new FileReader();
                var pipeline = CreatePipeline(args, reader, logger);
                var matches = FindMatches(args, reader, pipeline);

                WriteMatchesToFile(args, matches, logger);
            }
            catch(Exception ex)
            {
                logger.Log("Exception occurred" +  ex.Message);
            }

            logger.Log("End");
            logger.StopLogging();
        }

        private static bool IsMono()
        {
            return (Type.GetType("Mono.Runtime") != null);
        }

        private static ListingsToProductResolutionPipeline CreatePipeline(string[] args, FileReader reader, Logger logger)
        {
            var configFileName = ParseKeyValueFromArgsOrThrow(args, "--config-file");
            var config = String.Concat(reader.ReadLines(configFileName));

            var eratesFileName = ParseKeyValueFromArgsOrThrow(args, "--exchange-rate-file");
            var erates = reader.ReadLines(eratesFileName);

            var cameraTrainingSetFileName = ParseKeyValueFromArgsOrThrow(args, "--camera-training-set-file");
            var cameraTrainingSet = reader.ReadLines(cameraTrainingSetFileName);

            var accessoryTrainingSetFileName = ParseKeyValueFromArgsOrThrow(args, "--accessory-training-set-file");
            var accessoryTrainingSet = reader.ReadLines(accessoryTrainingSetFileName);

            return new ListingsToProductResolutionPipeline(logger.Log, config, erates, cameraTrainingSet, accessoryTrainingSet);
        }

        private static IEnumerable<ProductMatchDto> FindMatches(string[] args, FileReader reader, ListingsToProductResolutionPipeline pipeline)
        {
            var productsFileName = ParseKeyValueFromArgsOrThrow(args, "--products-file");
            var products = reader.ReadLines(productsFileName);

            var listingsFileName = ParseKeyValueFromArgsOrThrow(args, "--listings-file");
            var listings = reader.ReadLines(listingsFileName);

            return pipeline.FindMatches(products, listings);
        }

        private static string ParseKeyValueFromArgsOrThrow(string[] args, string key)
        {
            var i = args.FindIndex(x => x == key);
            if (i >= 0 && (i + 1) < args.Length)
            {
                return args[i + 1];
            }
            else
            {
                throw new ArgumentException(String.Format("Expected {0} in arguments.", key));
            }
        }

        private static void WriteMatchesToFile(string[] args, IEnumerable<ProductMatchDto> matches, Logger logger)
        {
            var isPrettyPrint = args.Contains("--pretty-print");
            logger.Log(String.Format("Writing results, Pretty Print {0}", isPrettyPrint));

            var outputFormatting = isPrettyPrint ? Formatting.Indented : Formatting.None;
            var output = matches.Select(x => JsonConvert.SerializeObject(x, outputFormatting));

            var writer = new FileWriter();
            writer.WriteLines(@"./results.txt", output);
        }
    }
}