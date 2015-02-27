using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using SteamTrade;
using SteamTrade.TradeOffer;

namespace KeyBot.Price
{
    public class PriceCache
    {
        private class Key
        {
            public readonly int AppId;
            public readonly string MarketHashName;
            public readonly int Currency;

            public Key(int appId, string name, int currency)
            {
                AppId = appId;
                MarketHashName = name;
                Currency = currency;
            }

            public override int GetHashCode()
            {
                return AppId ^MarketHashName.GetHashCode() ^ Currency;
            }

            public override bool Equals(object obj)
            {                               
                return Equals(obj as Key);
            }

            public bool Equals(Key k)
            {
                return k != null && k.AppId == AppId && k.MarketHashName == MarketHashName && k.Currency == Currency;
            }
        }

        private struct Price
        {
            public readonly DateTime Created;
            public readonly decimal Value;

            public Price(decimal value)
            {
                Created = DateTime.UtcNow;
                Value = value;
            }
        }

        private PriceChecker PriceChecker;
        private TimeSpan ExpirationTime;
        private Dictionary<Key, Price> Prices;

        public PriceCache(PriceChecker c, TimeSpan expirationTime)
        {
            PriceChecker = c;
            ExpirationTime = expirationTime;
            Prices = new Dictionary<Key, Price>();
        }

        public decimal? GetPrice(AssetDescription desc, int currency)
        {
            Price res;
            Key key = new Key(desc.AppId, desc.MarketHashName, currency);
            if (Prices.TryGetValue(key, out res) && DateTime.UtcNow - res.Created < ExpirationTime){
                return res.Value;
            } else {
                decimal? newPrice = PriceChecker.GetPrice(desc, currency);
                if (newPrice != null) {
                    Prices[key] = new Price(newPrice.Value);
                }
                return newPrice;
            }
        }
    }
}
