using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Classification;
using Pipeline.Domain;
using Pipeline.Shared;

namespace Pipeline.UnitTests.Classification
{
    [TestClass]
    public class NaiveBayesCameraClassifierTests
    {
        private static readonly string[] _cameraTrainingSet = new[]
        {
            "acme modela digital camera with zoom",
            "modelb digital camera 100 megapixels"
        };

        private static readonly string[] _accessoryTrainingSet = new[]
        {
            "battery for modela",
            "replacement battery for modelb",
            "case for modelb"
        };

        [TestMethod]
        public void WhenCamera_ExpectIsCamera()
        {
            var listing = new Listing { Title = "modelc digital camera" };
            var result = GetSut().IsCamera(listing);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WhenAccessory_ExpectIsNotCamera()
        {
            var listing = new Listing { Title = "replacement battery for modela 100mhr" };
            var result = GetSut().IsCamera(listing);
            Assert.IsFalse(result);
        }

        private static NaiveBayesCameraClassifier GetSut()
        {
            return new NaiveBayesCameraClassifier(_cameraTrainingSet, _accessoryTrainingSet, 3, 0.90F);
        }
    }
}