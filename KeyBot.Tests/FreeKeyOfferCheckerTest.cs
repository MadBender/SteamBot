using System;
using System.Collections.Generic;
using KeyBot.Models;
using KeyBot.OfferCheckers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KeyBot.Tests
{
    [TestClass]
    public class FreeKeyOfferCheckerTest: KeyOfferCheckerTest
    {
        public FreeKeyOfferCheckerTest()
        {
            Checker = new FreeKeyOfferChecker(
                    new HashSet<string>{
                        "Operation Phoenix Case Key", 
                        "Operation Breakout Case Key", 
                        "Huntsman Case Key"
                    });            
        }

        [TestMethod]
        public void CorrectOffer()
        {
            Assert.IsTrue(Checker.CheckOffer(
                new OfferModel {
                    ItemsToGive = new List<CEconAssetModel> { CsGoKey },
                    ItemsToReceive = new List<CEconAssetModel> { BreakoutKey }
                }));   
        }

        [TestMethod]
        public void WrongKeys()
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
        public void OffersAdds()
        {
            Assert.IsTrue(Checker.CheckOffer(
                new OfferModel {
                    ItemsToGive = new List<CEconAssetModel> { CsGoKey },
                    ItemsToReceive = new List<CEconAssetModel> { BreakoutKey, CsGoKey, OtherItem }
                }));
        }

        [TestMethod]
        public void WantsMyItems()
        {
            Assert.IsFalse(Checker.CheckOffer(
                new OfferModel {
                    ItemsToGive = new List<CEconAssetModel> { CsGoKey, OtherItem },
                    ItemsToReceive = new List<CEconAssetModel> { BreakoutKey }
                }));
        }

    }
}
