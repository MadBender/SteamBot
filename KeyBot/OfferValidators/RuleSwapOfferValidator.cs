using System.Collections.Generic;
using KeyBot.Models;

namespace KeyBot.OfferValidators
{
    public class RuleSwapOfferValidator: SwapOfferValidator
    {
        private SwapValidationRuleSet Rules;

        public RuleSwapOfferValidator(SwapValidationRuleSet rules)
        {
            Rules = rules;
        }

        protected override decimal? GetSwapPrice(CEconAssetModel mine, CEconAssetModel theirs)
        {
            return Rules.GetSwapPrice(
                mine.Description != null ? mine.Description.MarketHashName : null, 
                theirs.Description != null ? theirs.Description.MarketHashName : null
            );
        }        
    }
}
