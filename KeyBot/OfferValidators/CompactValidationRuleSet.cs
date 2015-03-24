using System.Collections.Generic;

namespace KeyBot.OfferValidators
{
    public class CompactValidationRuleSet
    {
        public Dictionary<string, HashSet<string>> Groups { get; set; }
        public List<SwapValidationRule> Rules { get; set; }

        public SwapValidationRuleSet GetFullRuleSet()
        {
            return new SwapValidationRuleSet {
                Rules = Rules.ConvertAll(r => new SwapValidationRule {
                    MyItems = ExpandGroups(r.MyItems),
                    TheirItems = ExpandGroups(r.TheirItems),
                    Price = r.Price
                })
            };
        }

        private HashSet<string> ExpandGroups(HashSet<string> items)
        {
            if (Groups == null) {
                return items;
            }

            HashSet<string> res = new HashSet<string>();
            foreach (string s in items) { 
                HashSet<string> group;
                if (Groups.TryGetValue(s, out group)) {
                    res.UnionWith(group);
                } else {
                    res.Add(s);
                }
            }
            return res;
        }
    }
}
