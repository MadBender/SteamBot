using AutoMapper;
using KeyBot.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamTrade.TradeOffer;

namespace KeyBot.Tests
{
    public static class Init
    {
        [AssemblyInitialize]
        public static void InitTests()
        {
            Mapper.CreateMap<CEconAsset, CEconAssetModel>();
        }
    }
}
