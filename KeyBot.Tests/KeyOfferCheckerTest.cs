using System;
using KeyBot.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SteamTrade.TradeOffer;

namespace KeyBot.Tests
{    
    public abstract class KeyOfferCheckerTest
    {
        public OfferModel GetOffer(string response)
        {
            OfferResponse o = JsonConvert.DeserializeObject<ApiResponse<OfferResponse>>(response).Response;
            return new OfferModel(o.Offer, o.Descriptions);
        }        
    }
}
