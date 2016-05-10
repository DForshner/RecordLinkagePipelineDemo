
namespace Pipeline.Shared
{
    /// <summary>
    /// Configuration for pipeline
    /// </summary>
    internal class Config
    {
        // Alias generation
        public int ManufacturerNameCutoff { get; set; }
        public float PossibleAliasPercentile { get; set; }
        public float CommonWordPercentile { get; set; }

        // Naive Bayes classifier
        public int MinNumWords { get; set; }
        public float WordRatio { get; set; }

        // Heuristic classifier
        public decimal LowPriceCutoff { get; set; }
        public decimal HighPriceCutoff { get; set; }
        public float Threshold { get; set; }

        // Price outlier classifier
        public decimal LowerRangeMultiplier { get; set; }
        public decimal UpperRangeMultiplier { get; set; }
    }
}
