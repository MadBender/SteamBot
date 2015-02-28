using System.Collections.Generic;
using KeyBot.Models;

namespace KeyBot.OfferCheckers
{
    public class KeySwapOfferChecker: SwapOfferChecker
    {
        private static HashSet<string> KeyNames = new HashSet<string>{
            "Operation Phoenix Case Key",
            "CS:GO Case Key",
            "Operation Breakout Case Key",
            "Huntsman Case Key",
            "eSports Key",
            "Winter Offensive Case Key",
            "Operation Vanguard Case Key",
            "Chroma Case Key"
        };

        private HashSet<string> FreeKeys;
        private decimal SwapPrice;

        public KeySwapOfferChecker(HashSet<string> freeKeys, decimal swapPrice)
        {
            FreeKeys = freeKeys;
            SwapPrice = swapPrice;
        }

        protected override decimal? GetSwapPrice(CEconAssetModel mine, CEconAssetModel theirs)
        {
            if (IsKey(mine) && IsKey(theirs)) {
                return FreeKeys.Contains(mine.Description.MarketHashName) && !FreeKeys.Contains(theirs.Description.MarketHashName)
                    ? 0m
                    : SwapPrice;
            }
            return null;
        }

        private bool IsKey(CEconAssetModel asset)
        {
            return asset.InstanceId == "143865972"
                && asset.AppId == "730"
                && asset.Description != null
                && KeyNames.Contains(asset.Description.MarketHashName);
        }
    }
}
