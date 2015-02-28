using System.Collections.Generic;
using KeyBot.Models;
using KeyBot.OfferValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamTrade.TradeOffer;

namespace KeyBot.Tests
{
    [TestClass]
    public class KeySwapOfferValidatorTest
    {
        private OfferValidator Checker;

        //sample objects

        private static CEconAssetModel CsGoKey = new CEconAssetModel {
            AppId = "730",
            InstanceId = "143865972",
            ClassId = "186150629",
            Description = new AssetDescription {
                MarketHashName = "CS:GO Case Key"
            },
            Price = 2.49m
        };

        private static CEconAssetModel BreakoutKey = new CEconAssetModel {
            AppId = "730",
            InstanceId = "143865972",
            ClassId = "613589848",
            Description = new AssetDescription {
                MarketHashName = "Operation Breakout Case Key"
            },
            Price = 2.49m
        };  

        public KeySwapOfferValidatorTest()
        {
            Checker = new KeySwapOfferValidator(
                new HashSet<string>{
                    "CS:GO Case Key",
                    "Winter Offensive Case Key",
                    "Chroma Case Key"
                }, 
                0.05m
            );            
        }      

        private CEconAssetModel Item(decimal? price)
        {
            return new CEconAssetModel { Price = price };
        }

        [TestMethod]
        public void CorrectOffer()
        {
            Assert.IsTrue(Checker.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel>{ BreakoutKey },
                ItemsToReceive = new List<CEconAssetModel>{ CsGoKey, Item(0.05m) }
            }));
        }

        [TestMethod]
        public void LowAdds()
        {
            Assert.IsFalse(Checker.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { BreakoutKey },
                ItemsToReceive = new List<CEconAssetModel> { CsGoKey, Item(0.04m) }
            }));
        }

        [TestMethod]
        public void MultipleAdds()
        {
            Assert.IsTrue(Checker.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { BreakoutKey },
                ItemsToReceive = new List<CEconAssetModel> { CsGoKey, Item(0.04m), Item(0.04m) }
            }));
        }

        [TestMethod]
        public void NoAdds()
        {
            Assert.IsFalse(Checker.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { BreakoutKey },
                ItemsToReceive = new List<CEconAssetModel> { CsGoKey }
            }));
        }

        [TestMethod]
        public void FreeSwap()
        {
            Assert.IsTrue(Checker.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { CsGoKey },
                ItemsToReceive = new List<CEconAssetModel> { BreakoutKey }
            }));
        }

        [TestMethod]
        public void WantsMyItems()
        {
            Assert.IsFalse(Checker.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { CsGoKey, Item(0.05m) },
                ItemsToReceive = new List<CEconAssetModel> { BreakoutKey }
            }));
        }

        [TestMethod]
        public void DonatesItems()
        {
            Assert.IsTrue(Checker.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> {  },
                ItemsToReceive = new List<CEconAssetModel> { Item(0.05m) }
            }));
        }

        [TestMethod]
        public void MixedSwap()
        {
            Assert.IsTrue(Checker.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { CsGoKey, BreakoutKey },
                ItemsToReceive = new List<CEconAssetModel> { BreakoutKey, BreakoutKey, Item(0.05m) }
            }));
        }

        [TestMethod]
        public void NullPrice()
        {
            Assert.IsTrue(Checker.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { BreakoutKey },
                ItemsToReceive = new List<CEconAssetModel> { CsGoKey, Item(null), Item(0.05m) }
            }));
        }

        [TestMethod]
        public void ExtraKeys()
        {
            Assert.IsTrue(Checker.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { BreakoutKey },
                ItemsToReceive = new List<CEconAssetModel> { CsGoKey, CsGoKey }
            }));
        }

        [TestMethod]
        public void LittleKeys()
        {
            Assert.IsFalse(Checker.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { CsGoKey, CsGoKey },
                ItemsToReceive = new List<CEconAssetModel> { BreakoutKey }
            }));
        }


    }
}
