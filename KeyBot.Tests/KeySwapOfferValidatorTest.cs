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
        private OfferValidator Validator;

        //sample objects

        private static CEconAssetModel CsGoKey = Item("CS:GO Case Key", 2.49m);
        private static CEconAssetModel BreakoutKey = Item("Operation Breakout Case Key", 2.49m);
        private static CEconAssetModel PhoenixKey = Item("Operation Phoenix Case Key", 2.49m);

        public KeySwapOfferValidatorTest()
        {
            Validator = new RuleSwapOfferValidator(
                SwapValidationRuleSetTest.GetRuleSet().GetFullRuleSet()
            );            
        }      

        private static CEconAssetModel Item(decimal? price)
        {
            return new CEconAssetModel { Price = price };
        }

        private static CEconAssetModel Item(string name, decimal? price)
        {
            return new CEconAssetModel { Description = new AssetDescription{ MarketHashName = name }, Price = price };
        }

        [TestMethod]
        public void CorrectOffer()
        {
            Assert.IsTrue(Validator.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel>{ BreakoutKey },
                ItemsToReceive = new List<CEconAssetModel>{ CsGoKey, Item(0.05m) }
            }));
        }

        [TestMethod]
        public void LowAdds()
        {
            Assert.IsFalse(Validator.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { BreakoutKey },
                ItemsToReceive = new List<CEconAssetModel> { CsGoKey, Item(0.04m) }
            }));
        }

        [TestMethod]
        public void MultipleAdds()
        {
            Assert.IsTrue(Validator.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { BreakoutKey },
                ItemsToReceive = new List<CEconAssetModel> { CsGoKey, Item(0.04m), Item(0.04m) }
            }));
        }

        [TestMethod]
        public void NoAdds()
        {
            Assert.IsFalse(Validator.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { BreakoutKey },
                ItemsToReceive = new List<CEconAssetModel> { CsGoKey }
            }));
        }

        [TestMethod]
        public void FreeSwap()
        {
            Assert.IsTrue(Validator.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { CsGoKey },
                ItemsToReceive = new List<CEconAssetModel> { BreakoutKey }
            }));
        }

        [TestMethod]
        public void WantsMyItems()
        {
            Assert.IsFalse(Validator.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { CsGoKey, Item(0.05m) },
                ItemsToReceive = new List<CEconAssetModel> { BreakoutKey }
            }));
        }

        [TestMethod]
        public void DonatesItems()
        {
            Assert.IsTrue(Validator.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> {  },
                ItemsToReceive = new List<CEconAssetModel> { Item(0.05m) }
            }));
        }

        [TestMethod]
        public void MixedSwap()
        {
            Assert.IsTrue(Validator.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { CsGoKey, PhoenixKey },
                ItemsToReceive = new List<CEconAssetModel> { BreakoutKey, BreakoutKey, Item(0.05m) }
            }));
        }

        [TestMethod]
        public void NullPrice()
        {
            Assert.IsTrue(Validator.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { BreakoutKey },
                ItemsToReceive = new List<CEconAssetModel> { CsGoKey, Item(null), Item(0.05m) }
            }));
        }

        [TestMethod]
        public void ExtraKeys()
        {
            Assert.IsTrue(Validator.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { BreakoutKey },
                ItemsToReceive = new List<CEconAssetModel> { CsGoKey, CsGoKey }
            }));
        }

        [TestMethod]
        public void LittleKeys()
        {
            Assert.IsFalse(Validator.IsValid(new OfferModel {
                ItemsToGive = new List<CEconAssetModel> { CsGoKey, CsGoKey },
                ItemsToReceive = new List<CEconAssetModel> { BreakoutKey }
            }));
        }


    }
}
