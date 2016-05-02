using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pipeline.Shared;

namespace Pipeline.Matching
{
    public class ProductMatcher
    {
        public IEnumerable<ProductMatch> Match(IEnumerable<Listing> toMatch, IEnumerable<string> canonicalManufacturerNames)
        {
            throw new NotImplementedException();
        }
    }
}