using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Analysis;
using Pipeline.Extraction;
using Pipeline.Matching;
using Pipeline.Shared;

namespace Pipeline.UnitTests.Matching
{
    [TestClass]
    public class ProductModelMatcherTests
    {
        [TestMethod]
        public void WhenProductModelMatchesListingTitle_ExpectMatched()
        {
            var listingBlock = new ManufacturerNameListingsBlock
            {
                Listings = new[]
                {
                    new Listing { Title = "ModelA" }
                }
            };
            var productBlock = new ManufacturerNameProductsBlock
            {
                Products = new[]
                {
                    new Product { Model = "ModelA" },
                    new Product { Model = "ModelB" },
                }
            };
            var termProbablities = new Dictionary<string, float>
            {
                { "ModelA", 0.1F },
                { "ModelB", 0.1F },
            };

            var result = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1.Single();

            Assert.AreEqual("ModelA", result.Product.Model);
            Assert.AreEqual("ModelA", result.Listings.Single().Title);
        }

        [TestMethod]
        public void WhenAllModelHasMultipleParts_ExpectAllWordsPresentInListing()
        {
            var matchingListingTitle = "pdr m60 2 3mp digital camera w 2x optical zoom";
            var listingBlock = new ManufacturerNameListingsBlock
            {
                Listings = new[]
                {
                    // No match
                    new Listing { Title = "pdr 3320 3 2mp digital camera w 3x optical zoom" },
                    new Listing { Title = "pdr m61 2mp digital camera" },
                    new Listing { Title = "pdr 4300 4mp digital camera w 2 8x optical zoom" },
                    new Listing { Title = "pdr 2300 2mp digital camera w 3x optcial zoom" },
                    new Listing { Title = "pdr t10 2mp digital camera" },
                    new Listing { Title = "pdr m81 4mp digital camera with 2 8x optical zoom" },
                    new Listing { Title = "pdr m25 2mp digital camera w 3x optical zoom" },
                    new Listing { Title = "pdr 5300 5mp digital camera w 3x optical zoom" },

                    new Listing { Title = matchingListingTitle }
                }
            };
            var productBlock = new ManufacturerNameProductsBlock
            {
                Products = new[]
                {
                    new Product { Model = "pdr m60" },
                }
            };
            var termProbablities = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listingBlock.Listings);

            var results = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1.Single();

            Assert.AreEqual(1, results.Listings.Count);
            Assert.AreEqual(matchingListingTitle, results.Listings.Single().Title);
        }

        [TestMethod]
        public void WhenAllModelHasNumberThenLetters_ExpectAllWordsOrderMattersInMatch()
        {
            var matchingListingTitle = FieldMunger.Munge("CANON PowerShot SX130 IS - silver");
            var listingBlock = new ManufacturerNameListingsBlock
            {
                Listings = new[]
                {
                    // match
                    new Listing { Title = matchingListingTitle },

                    // not a match
                    new Listing { Title = FieldMunger.Munge("Canon EOS 60D Digital SLR Camera with EF-S 18-135mm IS USM Lens & 70-300mm IS USM Lens + 16GB Card + Battery + 2 UV/FLD/CPL Filter Sets + Tripod + Lens Case + Accessory Kit") },
                    new Listing { Title = FieldMunger.Munge("Canon EOS 60D Digital SLR Camera with EF-S 18-135mm IS USM Lens & 70-300mm IS USM Lens + 16GB Card + Battery + 2 UV/FLD/CPL Filter Sets + Tripod + Lens Case + Accessory Kit") },
                    new Listing { Title = FieldMunger.Munge("EOS 550D + 18-55 IS + Lowepro (Kit 18-55 mm IS)") },
                }
            };
            var matchingProductModel = FieldMunger.Munge("SX130 IS");
            var productBlock = new ManufacturerNameProductsBlock
            {
                Products = new[]
                {
                    new Product { Model = FieldMunger.Munge("130 IS") },
                    new Product { Model = matchingProductModel }
                }
            };
            var termProbablities = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listingBlock.Listings);

            var results = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1.Single();

            Assert.AreEqual(1, results.Listings.Count);
            Assert.AreEqual(matchingListingTitle, results.Listings.Single().Title);
            Assert.AreEqual(matchingListingTitle, results.Listings.Single().Title);
        }

        [TestMethod]
        public void WhenModelPartsHyphenated_ExpectedMatch()
        {
            var listingBlock = new ManufacturerNameListingsBlock
            {
                Listings = new[]
                {
                    new Listing { Title = FieldMunger.Munge("PANASONIC Lumix DMC-FX70 - noir + Etui Pixmania Compact 11 X 3.5 X 8 CM NOIR + Carte mémoire SDHC 8 Go") },
                    new Listing { Title = FieldMunger.Munge("PANASONIC Lumix DMC-FX70 - noir + Etui Pixmania compact cuir 11 x 3,5 x 8 cm + Carte mémoire SDHC 16 Go") },
                    new Listing { Title = FieldMunger.Munge("PANASONIC Lumix DMC-FX70 - black") }
                }
            };
            var productBlock = new ManufacturerNameProductsBlock
            {
                Products = new[]
                {
                    new Product { Model = FieldMunger.Munge("dmc-fx70") },
                }
            };
            var termProbablities = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listingBlock.Listings);

            var results = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1.Single();

            Assert.AreEqual(3, results.Listings.Count);
        }

        [TestMethod]
        public void WhenModelPartsConcatinated_ExpectMatched()
        {
            var listingBlock = new ManufacturerNameListingsBlock
            {
                Listings = new[]
                {
                    // Match
                    new Listing { Manufacturer = "MATCH", Title = FieldMunger.Munge("Sony Alpha DSLRA850 24.6MP Digital SLR Camera (Body Only)") },
                    new Listing { Manufacturer = "MATCH", Title = FieldMunger.Munge("Sony - DSLR-A850- Appareil photo reflex numrique - 24,6 Mpix - Ecran LCD 3\" - HDMI - Stabilisateur - Anti-poussire - Noir") },

                    // No match
                    new Listing { Title = FieldMunger.Munge("Sony DSLRA850Q Alpha Digital Camera 24.6 MP Full Frame Exmor CMOS Sensor + Lens (28-70 mm f2.8)") },
                    new Listing { Title = FieldMunger.Munge("SAMSUNG S850 8.1 MP DIGITAL CAMERA - BLACK") },
                    new Listing { Title = FieldMunger.Munge("HP PhotoSmart 850 4MP Digital Camera w/ 8x Optical Zoom") }
                }
            };
            var productBlock = new ManufacturerNameProductsBlock
            {
                Products = new[]
                {
                    new Product { Model = FieldMunger.Munge("DSLR-A850") },
                }
            };
            var termProbablities = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listingBlock.Listings);

            var results = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1.Single();

            Assert.AreEqual(2, results.Listings.Count);
            Assert.IsTrue(results.Listings.All(x => x.Manufacturer == "MATCH"));
        }

        private ProductModelMatcher GetSut()
        {
            return new ProductModelMatcher();
        }
    }
}