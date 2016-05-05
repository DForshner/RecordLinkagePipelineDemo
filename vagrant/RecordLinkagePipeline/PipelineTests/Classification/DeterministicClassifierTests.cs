using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipeline.Analysis;
using Pipeline.Classification;
using Pipeline.Extraction;
using Pipeline.Shared;

namespace Pipeline.UnitTests.Classification
{
    [TestClass]
    public class DeterministicClassifierTests
    {
        [TestMethod]
        public void WhenCamera_ExpectIsCamera()
        {
            var testCases = new[]
            {
                new Listing { Title = "Nikon D5000 Digital SLR Camera Body + 2 Extended Life Batteries + Battery Charger + 8 GB Memory Card + Card Reader + Tripod + Carrying Case + Starter Kit + Digital Flash and more!!", CurrencyCode = "usd", Price = 854.99M },
                new Listing { Title = "Canon EOS Rebel XS 10.1 Digital SLR Camera with EF-S 18-55mm IS & EF-S 55-250mm f/4-5.6 IS Lens + High Capacity Li-Ion Battery + 4 GB Memory Card + 6 Piece Accessory Kit + Camera Holster Case + Multi-Coated Glass UV Filter + Multi-Coated Glass Polarizer Filter + 3 Year Warranty Repair Contract", CurrencyCode = "usd", Price = 849.95M },
                new Listing { Title = "AgfaPhoto OPTIMA 102 Digitalkamera (12 Megapixel, 3-fach opt. Zoom, 3 Zoll Display, bildstabilisiert) rot", CurrencyCode = "eur", Price = 66.61M },
                new Listing { Title = "Canon EOS Rebel T2i 18 MP CMOS APS-C Digital SLR Camera w/ EF-S 18-55mm f/3.5-5.6 IS Lens DavisMAX LPE8 Battery UV 16GB Backpack Bundle", CurrencyCode = "usd", Price = 989.99M },
            };
            var stub = new Dictionary<string, float>();

            var sut = GetSut();
            foreach(var listing in testCases)
            {
                var munged = new Listing { Title = FieldMunger.Munge(listing.Title), Price = listing.Price, CurrencyCode = FieldMunger.Munge(listing.CurrencyCode) }; 
                var result = sut.ClassifyAsCamera(stub, munged);
                Assert.IsTrue(result, "Expected [" + listing.Title + "] to be a camera.");
            }
        }

        [TestMethod]
        public void WhenAccessoryExpectIsNotCamera()
        {
            var testCases = new[]
            {
                new Listing { Title = "NIKON D300s (camera body only) + UP-Rise 34 Shoulder Bag + 16 GB SDHC Memory Card + Battery EN-EL3e", CurrencyCode = "usd", Price = 1781.41M },
                new Listing { Title = "Lithium-Ion Battery for Canon NB-4L Digital Camera", CurrencyCode = "cad", Price = 1.08M },
                new Listing { Title = "Canon LP-E6 Battery for Canon EOS 5D Mark II 7D LPE6", CurrencyCode = "cad", Price = 54.88M },
                new Listing { Title = "Genuine Canon BP-511A Battery Pack for EOS 5D, 50D, 40D, 30D, 20D, 10D, D. Rebel", CurrencyCode = "usd", Price = 39.88M },
                new Listing { Title = "Nikon EN-EL9a 1080mAh Ultra High Capacity Li-ion Battery Pack for Nikon D40, D40x, D60, D3000, & D5000 Digital SLR Cameras", CurrencyCode = "cad", Price = 29.75M },
                new Listing { Title = "DURAGADGET Padded Camera Bag With Shoulder Strap & Zip Pockets For Go Pro Hero HD Head Cams (Helmet Hero, Motorsports Hero, Surf Hero)", CurrencyCode = "cad", Price = 29.88M },
                new Listing { Title = "Canon Starter Kit for EOS Digital Rebel T2i SLR Camera, with Gadget Bag, Battery & Mini-HDMI Cable", CurrencyCode = "usd", Price = 85.77M },
            };
            var stub = new Dictionary<string, float>();

            var sut = GetSut();
            foreach(var listing in testCases)
            {
                var munged = new Listing { Title = FieldMunger.Munge(listing.Title), Price = listing.Price, CurrencyCode = FieldMunger.Munge(listing.CurrencyCode) };
                var result = sut.ClassifyAsCamera(stub, munged);
                Assert.IsFalse(result, "Expected [" + listing.Title + "] to be a battery.");
            }
        }

        private static DeterministicClassifier GetSut()
        {
            return new DeterministicClassifier();
        }
    }
}