using System.IO;
using KeyBot.OfferValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace KeyBot.Tests
{
    [TestClass]
    public class SwapValidationRuleSetTest
    {
        SwapValidationRuleSet RuleSet;

        public SwapValidationRuleSetTest()
        {
            RuleSet = GetRuleSet().GetFullRuleSet();
        }

        internal static CompactValidationRuleSet GetRuleSet()
        {
            return JsonConvert.DeserializeObject<CompactValidationRuleSet>(File.ReadAllText(Path.Combine(Init.CurrentDirectory, "SwapRules.json")));
        }

        [TestMethod]
        public void ParsingTest()
        {
            var ruleSet = GetRuleSet();
            Assert.AreEqual(1, ruleSet.Groups.Count);
            Assert.AreEqual(5, ruleSet.Rules.Count);
        }

        [TestMethod]
        public void PriceTest()
        {
            Assert.AreEqual(0m, RuleSet.GetSwapPrice("CS:GO Case Key", "Chroma Case Key"));
            Assert.AreEqual(0.05m, RuleSet.GetSwapPrice("Chroma Case Key", "CS:GO Case Key"));
            Assert.AreEqual(0m, RuleSet.GetSwapPrice("Chroma Case Key", "Operation Breakout Case Key"));
            Assert.AreEqual(0m, RuleSet.GetSwapPrice("Operation Breakout Case Key", "Operation Phoenix Case Key"));
            Assert.AreEqual(0.05m, RuleSet.GetSwapPrice("Huntsman Case Key", "eSports Key"));
            Assert.AreEqual(null, RuleSet.GetSwapPrice("eSports Key", "eSports Key"));
            Assert.AreEqual(null, RuleSet.GetSwapPrice("crap", "eSports Key"));
            Assert.AreEqual(null, RuleSet.GetSwapPrice("eSports Key", "crap"));
            Assert.AreEqual(null, RuleSet.GetSwapPrice("crap", "crap"));
        }
    }
}
