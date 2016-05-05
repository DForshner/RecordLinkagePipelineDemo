using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Analysis;
using Pipeline.Classification;
using Pipeline.Shared;

namespace Pipeline.UnitTests.Classification
{
    [TestClass]
    public class TermUniquenessDistributionClassifierTests
    {
        [TestMethod]
        public void WhenOneUniqueModelNumberAndManyCommon_ExpectIsCamera()
        {
            var camera = new Listing { Title = "camera zoom model7" };
            var listings = new[]
            {
                // Cameras
                new Listing { Title = "camera zoom model5" },
                new Listing { Title = "camera zoom model6" },
                new Listing { Title = "camera zoom model7 lens" },
                new Listing { Title = "camera zoom model8 lens" },
                new Listing { Title = "camera zoom model9 lens" },
                new Listing { Title = "camera zoom model10 lens" },
                camera,

                // Accessories
                new Listing { Title = "green bag" },
                new Listing { Title = "lens for model7 model6 model5" },
            };
            var probablityPerToken = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listings);

            var result = new TermUniquenessDistributionClassifier().ClassifyAsCamera(probablityPerToken, camera);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WhenManyUnique_ExpectAccessory()
        {
            var accessory = new Listing { Title = "15x lens model5 model6 model9" };
            var listings = new[]
            {
                // Cameras
                new Listing { Title = "camera zoom 15x lens model5" },
                new Listing { Title = "camera 15x lens model6" },
                new Listing { Title = "camera zoom 20x lens model7" },
                new Listing { Title = "camera 25x lens model8" },
                new Listing { Title = "camera zoom 25x lens model8" },

                // Accessories
                new Listing { Title = "green bag" },
                new Listing { Title = "lens for model7 model6 model5" },
                accessory
            };
            var probablityPerToken = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listings);

            var result = new TermUniquenessDistributionClassifier().ClassifyAsCamera(probablityPerToken, accessory);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WhenBattery_ExpectNotCamera()
        {
            var listings = new[]
            {
                new Listing { Title = "Energizer No. CHM39 - Battery charger 4xAA/AAA, 1x9V - included batteries: 4 x AA NiMH 1850 mAh", CurrencyCode = "gbp", Price = 32.01M },
            };
            //var probablityPerToken = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listings);
            //var result = new TermUniquenessDistributionPruner().ClassifyAsCamera(probablityPerToken, accessory);
            //Assert.IsFalse(result);
        }
    }
}