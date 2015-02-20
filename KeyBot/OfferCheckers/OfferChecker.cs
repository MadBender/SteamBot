using System.Collections.Generic;
using System.Linq;
using KeyBot.Models;

namespace KeyBot.OfferCheckers
{
    internal abstract class OfferChecker
    {
        public abstract bool CheckOffer(OfferModel o);
    }

    internal abstract class KeyOfferChecker: OfferChecker
    {
        protected static HashSet<string> KeyNames = new HashSet<string>{
            "Operation Phoenix Case Key",
            "CS:GO Case Key",
            "Operation Breakout Case Key",
            "Huntsman Case Key",
            "eSports Key",
            "Winter Offensive Case Key",
            "Operation Vanguard Case Key",
            "Chroma Case Key"
        };

        protected const string KeyInstanceId = "143865972";
        protected const string KeyAppId = "730";

        protected bool IsKey(CEconAssetModel asset)
        {
            return asset.InstanceId == KeyInstanceId
                && asset.AppId == KeyAppId
                && asset.Description != null
                && KeyNames.Contains(asset.Description.MarketName);
        }
    }

    internal class FeeKeyOfferChecker : KeyOfferChecker
    {
        public override bool CheckOffer(OfferModel o)
        {        
            int myKeyCount = o.ItemsToGive.Count(IsKey);            
            int theirKeyCount = o.ItemsToReceive.Count(IsKey);            
            return myKeyCount == o.ItemsToGive.Count
                && theirKeyCount >= myKeyCount
                && theirKeyCount < o.ItemsToReceive.Count;
        }
    }

    internal class FreeKeyOfferChecker : KeyOfferChecker
    {
        private HashSet<string> KeysToAccept;
        private HashSet<string> KeysToGive;
        public FreeKeyOfferChecker(HashSet<string> keysToAccept)
        {
            KeysToAccept = keysToAccept;
            KeysToGive = new HashSet<string>(KeyNames);
            KeysToGive.ExceptWith(KeysToAccept);
        }

        public override bool CheckOffer(OfferModel o)
        {            
            int myKeyCount = o.ItemsToGive.Count(
                a => a.AppId == KeyAppId 
                    && a.InstanceId == KeyInstanceId 
                    && KeysToGive.Contains(a.ClassId)
                );
            int theirKeyCount = o.ItemsToReceive.Count(
                a => a.AppId == KeyAppId 
                    && a.InstanceId == KeyInstanceId 
                    && KeysToAccept.Contains(a.ClassId)
                );        
            return theirKeyCount >= myKeyCount && myKeyCount == o.ItemsToGive.Count;
        }       
    }
}
