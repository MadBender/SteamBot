using System;
using KeyBot.Models;
using KeyBot.OfferCheckers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KeyBot.Tests
{
    [TestClass]
    public class FeeKeyOfferCheckerTest:KeyOfferCheckerTest
    {
        private FeeKeyOfferChecker Checker;

        public FeeKeyOfferCheckerTest()
        {
            Checker = new FeeKeyOfferChecker();
        }


        [TestMethod]
        public void NoAdds()
        {
            OfferModel o = GetOffer(Resources.FeeKeyOfferChecker.NoAdds);
            Assert.IsFalse(Checker.CheckOffer(o));
        }
    }
}
