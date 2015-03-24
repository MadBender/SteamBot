using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyBot.OfferValidators
{
    public class SwapValidationRuleSet
    {       
        public List<SwapValidationRule> Rules { get; set; }

        public decimal? GetSwapPrice(string myItem, string theirItem)
        {
            if (myItem != null && theirItem != null && myItem != theirItem) {
                foreach (SwapValidationRule r in Rules) {
                    if (r.MyItems.Contains(myItem) && r.TheirItems.Contains(theirItem)) {
                        return r.Price;
                    }
                }
            }
            return null;
        }
    }
}
