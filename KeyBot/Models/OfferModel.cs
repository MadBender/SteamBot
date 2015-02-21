using System.Collections.Generic;
using AutoMapper;
using SteamTrade.TradeOffer;

namespace KeyBot.Models
{
    public class OfferModel
    {
        public string TradeOfferId { get; set; }
        public List<CEconAssetModel> ItemsToGive { get; set; }
        public List<CEconAssetModel> ItemsToReceive { get; set; }

        public OfferModel()
        { }

        public OfferModel(Offer o, List<AssetDescription> descriptions)
        {
            TradeOfferId = o.TradeOfferId;
            ItemsToGive = GetAssetModels(o.ItemsToGive, descriptions);
            ItemsToReceive = GetAssetModels(o.ItemsToReceive, descriptions);
        }

        private List<CEconAssetModel> GetAssetModels(List<CEconAsset> assets, List<AssetDescription> descriptions)
        {            
            descriptions = descriptions ?? new List<AssetDescription>();
            assets = assets ?? new List<CEconAsset>();

            var res = Mapper.Map<List<CEconAsset>, List<CEconAssetModel>>(assets);
            foreach (CEconAssetModel a in res) {
                a.Description = descriptions
                    .Find(d => d.AppId == int.Parse(a.AppId)
                                && d.ClassId == a.ClassId
                                && d.InstanceId == a.InstanceId
                    );
            }
            return res;
        }
    }
}
