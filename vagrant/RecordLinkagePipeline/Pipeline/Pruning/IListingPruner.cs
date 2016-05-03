using System.Collections.Generic;
using Pipeline.Shared;

namespace Pipeline.Pruning
{
    public interface IListingPruner
    {
        bool ClassifyAsCamera(IDictionary<string, float> probablityPerToken, Listing listing);
    }
}
