using System;
using System.Collections.Generic;
using System.Linq;
using Pipeline.Analysis;
using Pipeline.Shared;

namespace Pipeline
{
    public class SimplePipeline
    {
        public IEnumerable<ProductMatch> FindMatches(IEnumerable<Product> products, IEnumerable<Listing> listings)
        {
            var productByManu = new Dictionary<string, List<Product>>();
            foreach (var product in products)
            {
                if (!productByManu.ContainsKey(product.Manufacturer))
                {
                    productByManu.Add(product.Manufacturer, new List<Product>());
                }
                productByManu[product.Manufacturer].Add(product);
            }

            var listingByManu = new Dictionary<string, List<Listing>>();
            foreach (var name in productByManu.Keys)
            {
                listingByManu.Add(name, new List<Listing>());
            }

            foreach (var listing in listings)
            {
                var tokens = listing.Manufacturer.Split(null);
                foreach (var token in tokens)
                {
                    if (listingByManu.ContainsKey(token))
                    {
                        listingByManu[token].Add(listing);
                        break;
                    }
                }
            }

            foreach (var name in productByManu.Keys)
            {
                var p = productByManu[name].Where(x => x.Model != null && x.Family != null);
                var l = listingByManu[name];

                var IdxByWord = new Dictionary<string, int>();
                var weights = new List<double>();
                var idx = 0;

                var modelFreq = new WordFrequencyPerCollection().Count(p, x => x.Model);
                foreach (var model in modelFreq)
                {
                    IdxByWord.Add(model.Key, idx);
                    weights.Add(model.Value);
                    idx++;
                }

                var familyFreq = new WordFrequencyPerCollection().Count(p, x => x.Family);
                foreach (var model in familyFreq)
                {
                    IdxByWord.Add(model.Key, idx);
                    weights.Add(model.Value);
                    idx++;
                }

                var productVector = new List<List<bool>>();
                foreach (var product in p)
                {
                    productVector.Add(new List<bool>());
                }
            }

            //var manuMatches = new ManufacturerMatcher().Match(listings, canonicalManufacturerNames).ToList();

            //var bad = manuMatches.Where(x => x.ManufacturerName == null);
            //var blah = new ManufacturerMatcher().Match(bad.Select(x => x.Listing), canonicalManufacturerNames).ToList();

            throw new NotImplementedException();
        }
    }
}
