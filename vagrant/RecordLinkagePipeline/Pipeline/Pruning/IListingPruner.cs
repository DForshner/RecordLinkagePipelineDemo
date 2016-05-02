using System.Collections.Generic;
using Pipeline.Shared;

namespace Pipeline.Pruning
{
    public interface IListingPruner
    {
        IEnumerable<Listing> Prune(ICollection<Product> products, ICollection<Listing> listings);
    }
}
