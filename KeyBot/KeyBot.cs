using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using KeyBot.Log;
using KeyBot.Models;
using KeyBot.OfferValidators;
using KeyBot.Price;
using SteamKit2;
using SteamTrade;
using SteamTrade.TradeOffer;

namespace KeyBot
{
    internal class KeyBot
    {
        public SteamUser.LogOnDetails LogonDetails { get; set; }
        
        private string ApiKey;
        private TimeSpan UpdateInterval;
        private SteamClient SteamClient;
        private SteamWeb SteamWeb;
        private SteamUser SteamUser;
        private CallbackManager CallbackManager;        

        private string UniqueID;        
        private string UserNonce;

        private TradeOfferManager OfferManager;
        private TradeOfferWebAPI TradeWebApi;
        private HashSet<string> ProcessedOffers;
        private PriceCache PriceCache;

        private List<OfferValidator> OfferValidators;

        //bot job
        private ManualResetEventSlim StopEvent;
        private Thread TradeCheckingThread;
        private ManualResetEventSlim BotStoppedEvent;
        
        public EResult LogoffReason { get; private set; }

        private ILogger Log { get; set; }

        public KeyBot(SteamUser.LogOnDetails logonDetails, string apiKey, TimeSpan updateInterval, List<OfferValidator> validators, ILogger log)
        {
            LogonDetails = logonDetails;
            Log = log;            
            ApiKey = apiKey;
            UpdateInterval = updateInterval;
            SteamWeb = new SteamWeb();
            TradeWebApi = new TradeOfferWebAPI(ApiKey, SteamWeb);
            BotStoppedEvent = new ManualResetEventSlim();
            OfferValidators = validators;
        }        

        public void Start()
        {
            new Thread(() => {
                SteamClient = new SteamClient();                
                try {                    
                    SteamUser = SteamClient.GetHandler<SteamUser>();

                    CallbackManager = new CallbackManager(SteamClient);

                    // register a few callbacks we're interested in
                    // these are registered upon creation to a callback manager, which will then route the callbacks
                    // to the functions specified
                    CallbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
                    CallbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
                    CallbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
                    CallbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
                    // this callback is triggered when the steam servers wish for the client to store the sentry file
                    CallbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);
                    CallbackManager.Subscribe<SteamUser.LoginKeyCallback>(OnLoginKey);
                    CallbackManager.Subscribe<SteamUser.WebAPIUserNonceCallback>(OnWebAPIUserNonce);                    
                                      
                    StopEvent = new ManualResetEventSlim();
                    SteamClient.Connect();
                    while (!StopEvent.IsSet) {
                        // in order for the callbacks to get routed, they need to be handled by the manager
                        CallbackManager.RunWaitCallbacks();
                    }
                }
                catch (Exception e) {
                    Log.Log(LogLevel.Debug, "Main thread exception: " + e.Message);
                    Stop();
                }
            }).Start();            
        }

        public void Stop()
        {  
            StopEvent.Set();            
            if (TradeCheckingThread != null && Thread.CurrentThread != TradeCheckingThread) {
                TradeCheckingThread.Join();
            }            
            if (SteamClient.IsConnected) {
                SteamClient.Disconnect();
            }
            BotStoppedEvent.Set();
        }

        public void Wait()
        {
            BotStoppedEvent.Wait();
        }

        public void Logoff()
        {            
            SteamUser.LogOff();
            Stop();
        }
        
        #region Logon
        private void OnConnected(SteamClient.ConnectedCallback callback)
        {            
            if (callback.Result != EResult.OK) {
                Log.Log(LogLevel.Debug, "Unable to connect to Steam: " + callback.Result);
                Stop();
                return;
            }

            Log.Log(LogLevel.Debug, "Connected to Steam! Logging in " + LogonDetails.Username + "...");

            byte[] sentryHash = null;
            string sentryFileName = Path.Combine(Program.CurrentDirectory, "sentry.bin");
            if (File.Exists(sentryFileName)) {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes(sentryFileName);
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            LogonDetails.SentryFileHash = sentryHash;
            LogonDetails.ShouldRememberPassword = true;
            
            SteamUser.LogOn(LogonDetails);            
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Log.Log(LogLevel.Debug, "Disconnected from Steam");
            Stop();            
        }

        public bool IsAdditionalAuthCode(EResult r)
        {
            return r == EResult.AccountLogonDenied
                || r == EResult.AccountLogonDeniedNeedTwoFactorCode
                || r == EResult.AccountLoginDeniedNeedTwoFactor
                || r == EResult.TwoFactorCodeMismatch;
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK) {
                LogoffReason = callback.Result;
                if (IsAdditionalAuthCode(LogoffReason)) {
                    Console.WriteLine("This account is SteamGuard protected!");

                    if (LogoffReason == EResult.AccountLogonDenied) {
                        Console.Write("Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);
                        LogonDetails.AuthCode = Console.ReadLine().ToUpperInvariant();
                    } else {
                        Console.Write("Please enter your 2 factor auth code from your authenticator app: ");
                        LogonDetails.TwoFactorCode = Console.ReadLine().ToUpperInvariant();
                    }
                    return;
                } else {
                    Log.Log(LogLevel.Warning, string.Format("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult));
                    Stop();
                }
            } else {
                Log.Log(LogLevel.Debug, "Successfully logged on!");
                UserNonce = callback.WebAPIUserNonce;
            }
            // at this point, we'd be able to perform actions on Steam            
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {            
            LogoffReason = callback.Result;
            Log.Log(LogLevel.Debug, "Logged off from Steam: " + callback.Result);
            Stop();
        }

        private void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Log.Log(LogLevel.Debug, "Updating sentryfile...");

            byte[] sentryHash = CryptoHelper.SHAHash(callback.Data);

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"
            File.WriteAllBytes(Path.Combine(Program.CurrentDirectory, "sentry.bin"), callback.Data);

            // inform the steam servers that we're accepting this sentry file
            SteamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = callback.Data.Length,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

            Log.Log(LogLevel.Debug, "Sentryfile updated");
        }

        private void OnLoginKey(SteamUser.LoginKeyCallback callback)
        {
            LogonDetails.LoginKey = callback.LoginKey;
            SteamUser.AcceptNewLoginKey(callback);
            UniqueID = callback.UniqueID.ToString();
            UserWebLogon();            
        }       

        private void OnWebAPIUserNonce(SteamUser.WebAPIUserNonceCallback callback)
        {
            if (callback.Result == EResult.OK) {
                Log.Log(LogLevel.Debug, "Received new WebAPIUserNonce.");
                UserNonce = callback.Nonce;
                UserWebLogon();
            }
        }
        
        private void UserWebLogon()
        {
            bool authd = SteamWeb.Authenticate(UniqueID, SteamClient, UserNonce);
            if (authd) {
                Log.Log(LogLevel.Debug, "Web authenticated");
                Log.Log(LogLevel.Debug, "Starting trade checking");
                TradeCheckingThread = new Thread(TradeCheckingProc);
                TradeCheckingThread.Start();                
            } else {
                Log.Log(LogLevel.Warning, "Web authentication failed");
                Stop();
            }
        }

        #endregion Logon              

        private void TradeCheckingProc()
        {            
            ProcessedOffers = new HashSet<string>();
            OfferManager = new TradeOfferManager(ApiKey, SteamWeb);
            PriceCache = new PriceCache(new PriceChecker(SteamWeb), TimeSpan.FromMinutes(5));
                                   
            while (!StopEvent.IsSet) {
                try {                    
                    //Log("Checking trades");
                    CheckTrades();
                }
                catch (TradeAcceptException) { 
                    //terminate the loop                    
                    Logoff();
                }
                catch (Exception e) {
                    Log.Log(LogLevel.Debug, "Error while checking trades:\n" + ExceptionHelper.GetExceptionText(e, true, true));
                }
                StopEvent.Wait(UpdateInterval);
            }
        }

        private void CheckTrades()
        {            
            //http://api.steampowered.com/IEconService/GetTradeOffers/v1?key=XXXXXXXXXXXXXXXXXXXXXXXXXX&get_received_offers=1&active_only=1            
            OffersResponse offers = TradeWebApi.GetActiveTradeOffers(false, true, true);
            if (offers.TradeOffersReceived != null) {
                List<Offer> newOffers = offers.TradeOffersReceived.FindAll(o => o.TradeOfferState == TradeOfferState.TradeOfferStateActive && !ProcessedOffers.Contains(o.TradeOfferId));
                foreach (Offer o in newOffers) {
                    var offerModel = new OfferModel(o, offers.Descriptions);
                    GetPrices(offerModel);
                    CheckOffer(offerModel);
                }
            }
        }

        private void CheckOffer(OfferModel o)
        {            
            bool accept = OfferValidators.Any(c => c.IsValid(o));
            Log.Log(LogLevel.Info, 
                string.Format(
                    "Trade {0}\nWants:\n{1}\nOffers:\n{2}\n{3}\n",
                    o.TradeOfferId,
                    GetOfferText(o.ItemsToGive),
                    GetOfferText(o.ItemsToReceive),
                    accept ? "Accept" : "Ignore"
                )
            );
            
            if (accept) {
                //accept
                Log.Log(LogLevel.Info, "Accepting " + o.TradeOfferId);
                TradeOffer to = null;
                if (OfferManager.GetOffer(o.TradeOfferId, out to)) {
                    TradeOfferAcceptResponse acceptResp = to.Accept();
                    if (acceptResp.Accepted) {
                        Log.Log(LogLevel.Info, o.TradeOfferId + " accepted");
                    } else { 
                        TradeOfferState curState = TradeWebApi.GetOfferState(o.TradeOfferId);
                        Log.Log(LogLevel.Warning, string.Format("Can't accept {0}: {1}", o.TradeOfferId, acceptResp.TradeError ?? ""));
                        //do not add to processed, return and retry next time
                        //it seems there is Steam accept error for some reason
                        throw new TradeAcceptException(acceptResp.TradeError);
                    }
                } else {
                    Log.Log(LogLevel.Warning, "Offer " + o.TradeOfferId + " not found");
                }
            }
            ProcessedOffers.Add(o.TradeOfferId);
        }

        private void GetPrices(OfferModel offer)
        {
            //getting price once for every group
            var groups = offer.ItemsToGive.Concat(offer.ItemsToReceive)
                .Where(a => a.Description != null)
                .GroupBy(a => new { a.Description.AppId, a.Description.MarketHashName });

            foreach (var g in groups) {
                decimal? price = PriceCache.GetPrice(g.First().Description, 1);
                foreach (CEconAssetModel a in g) {
                    a.Price = price;
                }
            }
        }        

        private string GetOfferText(List<CEconAssetModel> assets)
        {            
            var groups = assets
                .GroupBy(a => new { a.AppId, a.ClassId, a.InstanceId })
                .OrderByDescending(g => g.Count());

            StringBuilder sb = new StringBuilder();

            decimal total = 0;
            foreach (var g in groups) { 
                var key = g.Key;
                CEconAssetModel first = g.First();
                if (first.Price != null) {
                    total += first.Price.Value * g.Count();
                }
                sb.AppendFormat(
                    "{0, 4} {1, -55} {2, 8:0.00} {3, 8:0.00}\n", 
                    g.Count(), 
                    first.Description != null ? first.Description.MarketHashName : "Unknown",
                    first.Price != null ? first.Price : null,
                    first.Price != null ? first.Price * g.Count() : null
                );
            }
            sb.AppendFormat("     Total: {0, 66:0.00}\n", total);

            return sb.ToString();
        }
    }
}
