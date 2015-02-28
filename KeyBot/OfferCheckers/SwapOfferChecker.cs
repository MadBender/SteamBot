using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeyBot.Models;

namespace KeyBot.OfferCheckers
{
    public abstract class SwapOfferChecker: OfferChecker
    {    
        //parameters: my item, their item, swap price (null if can't swap)
        protected abstract decimal? GetSwapPrice(CEconAssetModel mine, CEconAssetModel theirs);
        
        public override bool CheckOffer(OfferModel o)
        {
            var theirItems = new List<CEconAssetModel>(o.ItemsToReceive);
            decimal totalSwapPrice = 0;

            //searhing a cheapest swap pair for every item of mine
            foreach (CEconAssetModel myItem in o.ItemsToGive) {
                CEconAssetModel swapItem = null;
                decimal itemSwapPrice = decimal.MaxValue;
                //looking for pair
                foreach(CEconAssetModel theirItem in theirItems) {
                    decimal? sPrice = GetSwapPrice(myItem, theirItem);
                    if (sPrice != null && sPrice.Value < itemSwapPrice) {
                        swapItem = theirItem;
                        itemSwapPrice = sPrice.Value;
                        if (itemSwapPrice == 0) {
                            //free swap - no point to search further
                            break;
                        }
                    }
                }

                if (swapItem == null) {
                    //a pair for my item was not found - don't accept this offer
                    return false;
                }

                theirItems.Remove(swapItem);
                totalSwapPrice += itemSwapPrice;
            }

            //what left in theirItems is swap fee
            decimal addPrice = theirItems.Sum(it => it.Price != null ? it.Price.Value : 0);
            return addPrice >= totalSwapPrice;
        }
    }
}
