using System.Collections.Generic;
using System.Linq;
using SteamTrade.TradeOffer;

namespace KeyBot.OfferCheckers
{
    internal abstract class OfferChecker
    {
        public abstract bool CheckOffer(Offer o);
    }

    internal abstract class KeyOfferChecker: OfferChecker
    {
        protected static HashSet<string> KeyClassIds = new HashSet<string>{
            "360448780", //Phoenix key
            "186150629", //CSGO key
            "613589848", //Breakout key
            "506856210", //Huntsman key
            "186150630", //ESports key
            "259019412", //Winter Offensive key
            "638243112", //Vanguard key
            "721248158"  //Chroma key
        };

        protected const string KeyInstanceId = "143865972";
        protected const string KeyAppId = "730";

        protected bool IsKey(CEconAsset asset)
        {
            return KeyClassIds.Contains(asset.ClassId) && asset.InstanceId == KeyInstanceId && asset.AppId == KeyAppId;
        }
    }

    internal class FeeKeyOfferChecker : KeyOfferChecker
    {
        public override bool CheckOffer(Offer o)
        {
            List<CEconAsset> toGive = o.ItemsToGive ?? new List<CEconAsset>();
            List<CEconAsset> toReceive = o.ItemsToReceive ?? new List<CEconAsset>();
            int myKeyCount = toGive.Count(IsKey);            
            int theirKeyCount = toReceive.Count(IsKey);            
            return theirKeyCount >= myKeyCount && myKeyCount == toGive.Count && theirKeyCount < toReceive.Count;
        }
    }

    internal class FreeKeyOfferChecker : KeyOfferChecker
    {
        private HashSet<string> KeysToAccept;
        private HashSet<string> KeysToGive;
        public FreeKeyOfferChecker(HashSet<string> keysToAccept)
        {
            KeysToAccept = keysToAccept;
            KeysToGive = new HashSet<string>(KeyClassIds);
            KeysToGive.ExceptWith(KeysToAccept);
        }

        public override bool CheckOffer(Offer o)
        {
            List<CEconAsset> toGive = o.ItemsToGive ?? new List<CEconAsset>();
            List<CEconAsset> toReceive = o.ItemsToReceive ?? new List<CEconAsset>();
            int myKeyCount = toGive.Count(a => a.AppId == KeyAppId && a.InstanceId == KeyInstanceId && KeysToGive.Contains(a.ClassId));
            int theirKeyCount = toReceive.Count(a => a.AppId == KeyAppId && a.InstanceId == KeyInstanceId && KeysToAccept.Contains(a.ClassId));        
            return theirKeyCount >= myKeyCount && myKeyCount == toGive.Count;
        }       
    }
}
