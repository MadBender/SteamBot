using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using SteamTrade;
using SteamTrade.TradeOffer;

namespace KeyBot.Price
{
    public class PriceChecker
    {
        private class GetPriceResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
            [JsonProperty("median_price")]
            public string MedianPrice { get; set; }
        }

        private SteamWeb SteamWeb;

        public PriceChecker(SteamWeb steamWeb)
        {
            SteamWeb = steamWeb;
        }

        public decimal? GetPrice(AssetDescription desc, int currency)
        { 
            /*
            {
                success: true
                lowest_price: "2,41&#8364; "
                volume: "4,075"
                median_price: "2,44&#8364; "
            }
             */

            NameValueCollection keys = new NameValueCollection{ 
                { "country", "US" }, 
                { "currency", currency.ToString() }, 
                { "appid", desc.AppId.ToString() }, 
                { "market_hash_name", desc.MarketHashName } 
            };
            try {
                GetPriceResponse response = JsonConvert.DeserializeObject<GetPriceResponse>(SteamWeb.Fetch("http://" + SteamWeb.SteamCommunityDomain + "/market/priceoverview", "GET", keys));
                if (response.Success) {
                    string priceString = HttpUtility.HtmlDecode(response.MedianPrice).Replace(",", ".");
                    Match m = Regex.Match(priceString, @"\d+(\.\d+)?");
                    if (m.Success) {
                        return decimal.Parse(m.Captures[0].Value, CultureInfo.InvariantCulture);
                    }
                }
            } catch {                
            }           
            return null;
        }
    }
}
