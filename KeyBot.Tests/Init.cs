using System.IO;
using System.Reflection;
using AutoMapper;
using KeyBot.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamTrade.TradeOffer;

namespace KeyBot.Tests
{
    [TestClass]
    public static class Init
    {
        public static string CurrentDirectory { get; private set; }

        [AssemblyInitialize]
        public static void InitTests(TestContext c)
        {
            Mapper.CreateMap<CEconAsset, CEconAssetModel>();
            CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
