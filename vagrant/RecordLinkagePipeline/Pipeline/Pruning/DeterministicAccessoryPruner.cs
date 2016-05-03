using System.Collections.Generic;
using Pipeline.Shared;

namespace Pipeline.Pruning
{
    /// <summary>
    /// Removes accessory listings using deterministic rules.
    /// </summary>
    public class DeterministicAccessoryPruner : IListingPruner
    {
        public bool ClassifyAsCamera(IDictionary<string, float> probablityPerToken, Listing listing)
        {
            if (listing.Title.Contains("accessory"))
                return false;

            if (listing.Title.Contains("bag"))
                return false;

            if (listing.Title.Contains("case"))
                return false;

            return true;
        }
    }
}
