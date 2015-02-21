using System;
using System.Collections.Generic;
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
            Checker = new FreeKeyOfferChecker(
                    new HashSet<string>{
                        "Operation Phoenix Case Key", 
                        "Operation Breakout Case Key", 
                        "Huntsman Case Key"
                    });            
        }

        [TestMethod]
        public void CorrectOffer()
        {
            OfferModel o = GetOffer(Resources.FreeKeyOfferChecker.CorrectOffer);
            Assert.IsTrue(Checker.CheckOffer(o));
        }
    }
}
