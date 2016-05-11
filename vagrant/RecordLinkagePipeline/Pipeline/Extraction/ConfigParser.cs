using System;
using System.Diagnostics;
using Pipeline.Domain;
using Pipeline.Shared;

namespace Pipeline.Extraction
{
    internal static class ConfigParser
    {
        public static Config Parse(String str)
        {
            Debug.Assert(!String.IsNullOrEmpty(str));

            var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(str);
            return new Config
            {
                ManufacturerNameCutoff = int.Parse((string)jsonObj["manufacturer_name_cutoff"]),
                PossibleAliasPercentile = float.Parse((string)jsonObj["possible_alias_percentile"]),
                CommonWordPercentile = float.Parse((string)jsonObj["common_word_percentile"]),
                MinNumWords = int.Parse((string)jsonObj["min_num_words"]),
                WordRatio = float.Parse((string)jsonObj["word_ratio"]),
                LowPriceCutoff = decimal.Parse((string)jsonObj["low_price_cutoff"]),
                HighPriceCutoff = decimal.Parse((string)jsonObj["high_price_cutoff"]),
                Threshold = float.Parse((string)jsonObj["threshold"]),
                LowerRangeMultiplier = decimal.Parse((string)jsonObj["lower_range_multiplier"]),
                UpperRangeMultiplier = decimal.Parse((string)jsonObj["upper_range_multiplier"]),
            };
        }
    }
}
