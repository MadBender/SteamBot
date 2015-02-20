using System;
using KeyBot.Models;
using KeyBot.OfferCheckers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KeyBot.Tests
{
    [TestClass]
    public class FreeKeyOfferCheckerTest: KeyOfferCheckerTest
    {
        public FreeKeyOfferChecker Checker;

        public FreeKeyOfferCheckerTest()
        {
            Checker = new FeeKeyOfferChecker();
        }

        [TestMethod]
        public void CorrectOffer()
        {
            OfferModel o = GetOffer(Resources.FeeKeyOfferChecker.CorrectOffer);
            Assert.IsTrue(Checker.CheckOffer(o));
        }
    }
}
