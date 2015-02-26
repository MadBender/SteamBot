using System;
using System.Collections.Generic;
using KeyBot.Models;
using KeyBot.OfferCheckers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KeyBot.Tests
{
    [TestClass]
    public class FeeKeyOfferCheckerTest:KeyOfferCheckerTest
    {
        public FeeKeyOfferCheckerTest()
        {
            Checker = new FeeKeyOfferChecker();
        }

        [TestMethod]
        public void CorrectOffer()
        {
            Assert.IsTrue(Checker.CheckOffer(
                new OfferModel {
                    ItemsToGive = new List<CEconAssetModel> { BreakoutKey },
                    ItemsToReceive = new List<CEconAssetModel> { CsGoKey, OtherItem }
                }));            
        }

        [TestMethod]
        public void NoAdds()
        {
            Assert.IsFalse(Checker.CheckOffer(
                new OfferModel {
                    ItemsToGive = new List<CEconAssetModel> { BreakoutKey },
                    ItemsToReceive = new List<CEconAssetModel> { CsGoKey }
                }));            
        }

        [TestMethod]
        public void WantsMoreKeys()
        {
            Assert.IsFalse(Checker.CheckOffer(
                new OfferModel {
                    ItemsToGive = new List<CEconAssetModel> { BreakoutKey, BreakoutKey },
                    ItemsToReceive = new List<CEconAssetModel> { CsGoKey }
                }));            
        }

        [TestMethod]
        public void OffersMoreKeys()
        {
            Assert.IsTrue(Checker.CheckOffer(
                new OfferModel {
                    ItemsToGive = new List<CEconAssetModel> { BreakoutKey },
                    ItemsToReceive = new List<CEconAssetModel> { CsGoKey, CsGoKey }
                }));  
        }

        [TestMethod]
        public void WantsMyItems()
        {
            Assert.IsFalse(Checker.CheckOffer(
                new OfferModel {
                    ItemsToGive = new List<CEconAssetModel> { BreakoutKey, OtherItem },
                    ItemsToReceive = new List<CEconAssetModel> { CsGoKey, OtherItem }
                }));  
        }

        [TestMethod]
        public void ForgotMyItems()
        {
            Assert.IsTrue(Checker.CheckOffer(
                new OfferModel {
                    ItemsToGive = new List<CEconAssetModel> { },
                    ItemsToReceive = new List<CEconAssetModel> { CsGoKey, OtherItem }
                }));
        }

    }
}
