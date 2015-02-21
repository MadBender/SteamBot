using System;
using KeyBot.Models;
using KeyBot.OfferCheckers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SteamTrade.TradeOffer;

namespace KeyBot.Tests
{    
    public abstract class KeyOfferCheckerTest
    {
        protected KeyOfferChecker Checker;

        //sample objects

        protected static CEconAssetModel CsGoKey = new CEconAssetModel {
            AppId = "730",
            InstanceId = "143865972",
            ClassId = "186150629",
            Description = new AssetDescription {
                MarketHashName = "CS:GO Case Key"
            }
        };

        protected static CEconAssetModel BreakoutKey = new CEconAssetModel {
            AppId = "730",
            InstanceId = "143865972",
            ClassId = "613589848",
            Description = new AssetDescription {
                MarketHashName = "Operation Breakout Case Key"
            }
        };

        protected static CEconAssetModel OtherItem = new CEconAssetModel {
            AppId = "730",
            InstanceId = "188530139",
            ClassId = "469440491",
            Description = new AssetDescription {
                MarketHashName = "SSG 08 | Slashed (Field-Tested)"
            }
        };        
    }
}
