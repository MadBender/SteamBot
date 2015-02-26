using KeyBot.Price;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamTrade;
using SteamTrade.TradeOffer;

namespace KeyBot.Tests
{
    [TestClass]
    public class PriceCheckerTest
    {
        private SteamWeb SteamWeb;
        public PriceCheckerTest()
        {
            SteamWeb = new SteamWeb();
        }

        [TestMethod]
        public void TestPrice()
        {
            decimal? price = new PriceChecker(SteamWeb)
                .GetPrice(new AssetDescription {
                    MarketHashName = "CS:GO Case Key",
                    AppId = 730
                }, 1);

            Assert.IsNotNull(price);
        }
    }
}
