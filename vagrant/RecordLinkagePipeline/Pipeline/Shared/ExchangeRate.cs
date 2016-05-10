
namespace Pipeline.Shared
{
    internal class ExchangeRate
    {
        public string SourceCurrencyCode { get; set; }

        public string DestinationCurrencyCode { get; set; }

        public decimal Rate { get; set; }
    }
}
