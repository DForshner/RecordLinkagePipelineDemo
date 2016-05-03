using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Analysis;
using Pipeline.Pruning;
using Pipeline.Shared;

namespace Pipeline.UnitTests.Pruning
{
    [TestClass]
    public class TermUniquenessDistributionPrunerTests
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

            var result = new TermUniquenessDistributionPruner().ClassifyAsCamera(probablityPerToken, camera);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WhenManyUnique_ExpectAccessory()
        {
            var accessory = new Listing { Title = "camera zoom" };
            var listings = new[]
            {
                // Cameras
                new Listing { Title = "camera zoom model5" },
                new Listing { Title = "camera zoom model6" },
                new Listing { Title = "camera zoom model7" },

                // Accessories
                new Listing { Title = "green bag" },
                new Listing { Title = "lens for model7 model6 model5" },
                accessory
            };
            var probablityPerToken = TokenProbablityPerListingCalculator.GenerateTokenProbabilitiesPerListing(listings);

            var result = new TermUniquenessDistributionPruner().ClassifyAsCamera(probablityPerToken, accessory);

            Assert.IsFalse(result);
        }
    }
}