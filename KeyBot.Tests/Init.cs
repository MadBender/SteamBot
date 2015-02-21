using AutoMapper;
using KeyBot.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamTrade.TradeOffer;

namespace KeyBot.Tests
{
    [TestClass]
    public static class Init
    {
        [AssemblyInitialize]
        public static void InitTests(TestContext c)
        {
            Mapper.CreateMap<CEconAsset, CEconAssetModel>();
        }
    }
}
