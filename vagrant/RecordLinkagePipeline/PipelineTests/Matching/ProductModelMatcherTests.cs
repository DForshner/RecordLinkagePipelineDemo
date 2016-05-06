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
            var listingBlock = MakeListingBlock(new[]
            {
                MakeListing("modela")
            });
            var productBlock = MakeProductBlock(new[] { MakeProduct("modela"), MakeProduct("modelb") });
            var termProbablities = new Dictionary<string, float>
            {
                { "modela", 0.1F },
                { "modelb", 0.1F },
            };

            var result = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1.Single();

            Assert.AreEqual("modela", result.Product.Model);
            Assert.AreEqual("modela", result.Listings.Single().Title);
        }

        [TestMethod]
        public void WhenAllModelHasMultipleParts_ExpectAllWordsPresentInListing()
        {
            var matchingListingTitle = "pdr m60 2 3mp digital camera w 2x optical zoom";
            var listingBlock = MakeListingBlock(new[]
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
            });
            var productBlock = MakeProductBlock(new[] { MakeProduct("pdr m60") });
            var termProbablities = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listingBlock.Listings);

            var results = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1.Single();

            Assert.AreEqual(1, results.Listings.Count);
            Assert.AreEqual(matchingListingTitle, results.Listings.Single().Title);
        }

        [TestMethod]
        public void WhenAllModelHasNumberThenLetters_ExpectAllWordsOrderMattersInMatch()
        {
            var matchingListingTitle = FieldMunger.Munge("CANON PowerShot SX130 IS - silver");
            var listingBlock = MakeListingBlock(new[]
            {
                // match
                new Listing { Title = matchingListingTitle },

                // not a match
                MakeListing("Canon EOS 60D Digital SLR Camera with EF-S 18-135mm IS USM Lens & 70-300mm IS USM Lens + 16GB Card + Battery + 2 UV/FLD/CPL Filter Sets + Tripod + Lens Case + Accessory Kit"),
                MakeListing("Canon EOS 60D Digital SLR Camera with EF-S 18-135mm IS USM Lens & 70-300mm IS USM Lens + 16GB Card + Battery + 2 UV/FLD/CPL Filter Sets + Tripod + Lens Case + Accessory Kit"),
                MakeListing("EOS 550D + 18-55 IS + Lowepro (Kit 18-55 mm IS)"),
            });
            var matchingProductModel = FieldMunger.Munge("SX130 IS");
            var productBlock = MakeProductBlock(new[]
            {
                new Product { Model = FieldMunger.Munge("130 IS") },
                new Product { Model = matchingProductModel }
            });
            var termProbablities = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listingBlock.Listings);

            var results = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1.Single();

            Assert.AreEqual(1, results.Listings.Count);
            Assert.AreEqual(matchingListingTitle, results.Listings.Single().Title);
            Assert.AreEqual(matchingListingTitle, results.Listings.Single().Title);
        }

        [TestMethod]
        public void WhenModelPartsHyphenated_ExpectedMatch()
        {
            var listingBlock = MakeListingBlock(new[]
            {
                MakeListing("PANASONIC Lumix DMC-FX70 - noir + Etui Pixmania Compact 11 X 3.5 X 8 CM NOIR + Carte mémoire SDHC 8 Go"),
                MakeListing("PANASONIC Lumix DMC-FX70 - noir + Etui Pixmania compact cuir 11 x 3,5 x 8 cm + Carte mémoire SDHC 16 Go"),
                MakeListing("PANASONIC Lumix DMC-FX70 - black")
            });
            var productBlock = MakeProductBlock(new[] { MakeProduct("dmc-fx70") });
            var termProbablities = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listingBlock.Listings);

            var results = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1.Single();

            Assert.AreEqual(3, results.Listings.Count);
        }

        [TestMethod]
        public void WhenModelPartsConcatinated_ExpectMatched()
        {
            var listingBlock = MakeListingBlock(new[]
            {
                // Match
                new Listing { Manufacturer = "MATCH", Title = FieldMunger.Munge("Sony Alpha DSLRA850 24.6MP Digital SLR Camera (Body Only)"), },
                new Listing { Manufacturer = "MATCH", Title = FieldMunger.Munge("Sony - DSLR-A850- Appareil photo reflex numrique - 24,6 Mpix - Ecran LCD 3\" - HDMI - Stabilisateur - Anti-poussire - Noir") },

                // No match
                MakeListing("Sony DSLRA850Q Alpha Digital Camera 24.6 MP Full Frame Exmor CMOS Sensor + Lens (28-70 mm f2.8)"),
                MakeListing("SAMSUNG S850 8.1 MP DIGITAL CAMERA - BLACK"),
                MakeListing("HP PhotoSmart 850 4MP Digital Camera w/ 8x Optical Zoom")
            });
            var productBlock = MakeProductBlock(new[] { MakeProduct("DSLR-A850") });
            var termProbablities = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listingBlock.Listings);

            var results = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1.Single();

            Assert.AreEqual(2, results.Listings.Count);
            Assert.IsTrue(results.Listings.All(x => x.Manufacturer == "MATCH"));
        }

        [TestMethod]
        public void WhenModelLacksFamilyName_ExpectFamilyRequiredForJoin()
        {
            var listingBlock = MakeListingBlock(new[]
            {
                // Match
                MakeListing("Canon IXUS 105 Digital Camera - Aqua (12.1 MP, 4x Optical Zoom) 2.7 Inch PureColor LCD"),
                MakeListing("Canon IXUS 105 Digitalkamera (12 Megapixel, 4-fach opt. Zoom, 6.9 cm (2.7 Zoll) Display, bildstabilisiert) aqua"),

                // No match
                MakeListing("Canon EOS 5D Mark II 21.1MP Full Frame CMOS Digital SLR Camera with EF 24-105mm f/4 L IS USM Lens"),
                MakeListing("Canon EOS Digital Rebel T1i SLR Camera Dental Kit - Economy Version - Black Finish- - with Sigma 105/2.8 Macro Lens, Adorama Macro Ring Flash, Spare LP-E5 Type Battery"),
                MakeListing("AgfaPhoto OPTIMA 105 Digitalkamera (14 Megapixel, 3-fach opt. Zoom, 3 Zoll Display) schwarz"),
            });
            var productBlock = MakeProductBlock(new[] { MakeProduct("105", "IXUS") });
            var termProbablities = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listingBlock.Listings);

            var results = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1.Single();

            Assert.AreEqual(2, results.Listings.Count);
            Assert.IsTrue(results.Listings.All(x => x.Title.Contains("canon ixus 105")));
        }

        /// <summary>
        /// Fix bug where listings for same family but different model were being joined.
        /// </summary>
        [TestMethod]
        public void WhenModelLacksFamilyName_ExpectModelRequiredForJoin()
        {
            var listingBlock = MakeListingBlock(new[]
            {
                // No match
                MakeListing("canon powershot g10 appareil photo compact numérique ecran lcd 3 14 7 mp zoom optique x5 stabilisateur d image 8gb kit accessoires somptueux"),
                MakeListing("canon powershot a650is 12 1mp digital camera with 6x optical image stabilized zoom"),
                MakeListing("canon powershot a640 10mp digital camera with 4x optical zoom"),
            });
            var productBlock = MakeProductBlock(new[] { MakeProduct("600", "powershot") });
            var termProbablities = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listingBlock.Listings);
            var results = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1;
            Assert.AreEqual(0, results.Count());
        }

        [TestMethod]
        public void WhenModelLacksFamilyName_ExpectFamilyNameTokenOrderUsedInJoin()
        {
            var listingBlock = MakeListingBlock(new[]
            {
                // Match
                MakeListing("Leica D-LUX 5"),
                MakeListing("Leica 18151 D-Lux 5 Digital Camera"),

                // No match
                MakeListing("Leica V-LUX 2 14.1 MP Digital Camera with 4.5-108mm Leica Lens + 32GB Accessory Kit")
            });
            var productBlock = MakeProductBlock(new[] { MakeProduct("5", "D-LUX") });
            var termProbablities = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listingBlock.Listings);

            var results = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1.Single();

            Assert.AreEqual(2, results.Listings.Count());
            Assert.IsTrue(results.Listings.All(x => x.Title.Contains("d lux 5")));
        }

        /// <summary>
        /// Fix bug where listings for same family but different model were being joined.
        /// </summary>
        [TestMethod]
        public void WhenThreePartModelName_ExpectAllPartsPresentInTitle()
        {
            var listingBlock = MakeListingBlock(new[]
            {
                MakeListing("olympus pen e pl1 red m zuiko digital ed 14 42 mm lens expert shot backpack for digital cameras"),
            });
            var productBlock = MakeProductBlock(new[] { MakeProduct("pen e pl2") });
            var termProbablities = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listingBlock.Listings);
            var results = GetSut().FindProductMatchs(listingBlock, productBlock, termProbablities).Item1;
            Assert.AreEqual(0, results.Count());
        }

        private Product MakeProduct(string model = "", string family = "")
        {
            return new Product { Model = FieldMunger.Munge(model), Family = FieldMunger.Munge(family) };
        }

        private const string MANUFACTURER_NAME = "foo";

        private static ManufacturerNameListingsBlock MakeListingBlock(ICollection<Listing> listings)
        {
            return new ManufacturerNameListingsBlock(MANUFACTURER_NAME, listings);
        }

        private static ManufacturerNameProductsBlock MakeProductBlock(ICollection<Product> products)
        {
            return new ManufacturerNameProductsBlock(MANUFACTURER_NAME, products);
        }

        private Listing MakeListing(string title = "")
        {
            return new Listing { Title = FieldMunger.Munge(title) };
        }

        private ProductModelMatcher GetSut()
        {
            return new ProductModelMatcher();
        }
    }
}