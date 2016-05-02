using System.Collections.Generic;
using Pipeline.Shared;

namespace Pipeline.Pruning
{
    /// <summary>
    /// Removes accessory listings using deterministic rules.
    /// </summary>
    public class DeterministicAccessoryPruner
    {
        IEnumerable<Listing> Prune(ICollection<Product> products, ICollection<Listing> listings)
        {
            foreach(var listing in listings)
            {
                if (listing.Title.Contains("accessory"))
                    continue;

                if (listing.Title.Contains("bag"))
                    continue;

                if (listing.Title.Contains("case"))
                    continue;

                yield return listing;
            }
        }
    }
}
