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
            for (int i = 1; i <= 5; i++) {
                decimal? price = new PriceChecker(SteamWeb)
                    .GetPrice(new AssetDescription {
                        MarketHashName = "CS:GO Case Key",
                        AppId = 730
                    }, i);

                Assert.IsNotNull(price);
            }
        }
    }
}
