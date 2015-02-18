using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using KeyBot.OfferCheckers;
using SteamKit2;
using SteamTrade;
using SteamTrade.TradeOffer;

namespace KeyBot
{
    internal class KeyBot
    {
        private string Login;
        private string Password;
        private string ApiKey;
        private TimeSpan UpdateInterval;
        private SteamClient SteamClient;
        private SteamWeb SteamWeb;
        private SteamTrading SteamTrade;
        private SteamUser SteamUser;
        private CallbackManager CallbackManager;        

        public string TwoFactorAuth { get; set; }
        public string AuthCode { get;set; }

        private string UniqueID;        
        private string UserNonce;

        private TradeOfferManager OfferManager;
        private TradeOfferWebAPI TradeWebApi;
        private HashSet<string> ProcessedOffers;

        //bot job
        private ManualResetEventSlim StopEvent;
        private Thread TradeCheckingThread;
        private ManualResetEventSlim BotStoppedEvent;

        public EResult LogoffReason { get; private set; }

        public KeyBot(string login, string password, string apiKey, TimeSpan updateInterval)
        {
            Login = login;
            Password = password;
            ApiKey = apiKey;
            UpdateInterval = updateInterval;
            SteamWeb = new SteamWeb();
            TradeWebApi = new TradeOfferWebAPI(ApiKey, SteamWeb);
            BotStoppedEvent = new ManualResetEventSlim();
        }        

        public void Start()
        {
            TradeCheckingThread = new Thread(() => {
                SteamClient = new SteamClient();                
                try {
                    SteamTrade = SteamClient.GetHandler<SteamTrading>();
                    SteamUser = SteamClient.GetHandler<SteamUser>();

                    CallbackManager = new CallbackManager(SteamClient);

                    // register a few callbacks we're interested in
                    // these are registered upon creation to a callback manager, which will then route the callbacks
                    // to the functions specified
                    new Callback<SteamClient.ConnectedCallback>(OnConnected, CallbackManager);
                    new Callback<SteamClient.DisconnectedCallback>(OnDisconnected, CallbackManager);

                    new Callback<SteamUser.LoggedOnCallback>(OnLoggedOn, CallbackManager);
                    new Callback<SteamUser.LoggedOffCallback>(OnLoggedOff, CallbackManager);
                    // this callback is triggered when the steam servers wish for the client to store the sentry file
                    new Callback<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth, CallbackManager);
                    new Callback<SteamUser.LoginKeyCallback>(OnLoginKey, CallbackManager);
                    new Callback<SteamUser.WebAPIUserNonceCallback>(OnWebAPIUserNonce, CallbackManager);
                                        
                    StopEvent = new ManualResetEventSlim();
                    SteamClient.Connect();
                    while (!StopEvent.IsSet) {
                        // in order for the callbacks to get routed, they need to be handled by the manager
                        CallbackManager.RunWaitCallbacks();
                    }
                }
                catch (Exception e) {
                    Log("Main thread exception: " + e.Message);
                    Stop();
                }
            });
            TradeCheckingThread.Start();
        }

        public void Stop()
        {  
            StopEvent.Set();
            //do not call from trade checking thread
            if (TradeCheckingThread != null) {
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
                Log("Unable to connect to Steam: " + callback.Result);
                Stop();
                return;
            }

            Log("Connected to Steam! Logging in " + Login + "...");

            byte[] sentryHash = null;
            string sentryFileName = Path.Combine(Program.CurrentDirectory, "sentry.bin");
            if (File.Exists(sentryFileName)) {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes(sentryFileName);
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            SteamUser.LogOn(new SteamUser.LogOnDetails {
                Username = Login,
                Password = Password,

                // in this sample, we pass in an additional authcode
                // this value will be null (which is the default) for our first logon attempt
                AuthCode = AuthCode,

                // if the account is using 2-factor auth, we'll provide the two factor code instead
                // this will also be null on our first logon attempt
                TwoFactorCode = TwoFactorAuth,

                // our subsequent logons use the hash of the sentry file as proof of ownership of the file
                // this will also be null for our first (no authcode) and second (authcode only) logon attempts
                SentryFileHash = sentryHash,
            });            
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {            
            Log("Disconnected from Steam");
            Stop();            
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLogonDeniedNeedTwoFactorCode;

            if (isSteamGuard || is2FA) {
                Log("This account is SteamGuard protected!");

                if (is2FA) {
                    Console.Write("Please enter your 2 factor auth code from your authenticator app: ");
                    TwoFactorAuth = Console.ReadLine();
                } else {
                    Console.Write("Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);
                    AuthCode = Console.ReadLine().ToUpperInvariant();
                }                
                return;
            }

            if (callback.Result != EResult.OK) {
                Log(string.Format("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult));
                Stop();
                return;
            }

            Log("Successfully logged on!");
            UserNonce = callback.WebAPIUserNonce;

            // at this point, we'd be able to perform actions on Steam            
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {            
            LogoffReason = callback.Result;
            Log("Logged off from Steam: " + callback.Result);
            Stop();
        }

        private void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Log("Updating sentryfile...");

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

            Log("Done!");
        }

        private void OnLoginKey(SteamUser.LoginKeyCallback callback)
        {
            UniqueID = callback.UniqueID.ToString();
            UserWebLogon();
        }       

        private void OnWebAPIUserNonce(SteamUser.WebAPIUserNonceCallback callback)
        {
            if (callback.Result == EResult.OK) {
                Log("Received new WebAPIUserNonce.");
                UserNonce = callback.Nonce;
                UserWebLogon();
            }
        }
        
        private void UserWebLogon()
        {
            bool authd = SteamWeb.Authenticate(UniqueID, SteamClient, UserNonce);
            if (authd) {
                Log("Web authenticated");
                Log("Starting trade checking");
                new Thread(TradeCheckingProc).Start();                
            } else {
                Log("Web authentication failed");
            }
        }

        #endregion Logon              

        private void TradeCheckingProc()
        {            
            ProcessedOffers = new HashSet<string>();
            OfferManager = new TradeOfferManager(ApiKey, SteamWeb);

            List<OfferChecker> checkers = new List<OfferChecker>{
                new FeeKeyOfferChecker(),
                new FreeKeyOfferChecker(new HashSet<string>{"360448780", "613589848", "506856210"})
            };

            while (!StopEvent.IsSet) {
                try {                    
                    //Log("Checking trades");
                    CheckTrades(checkers);
                }
                catch (TradeAcceptException) { 
                    //terminate the loop                    
                    Logoff();
                }
                catch (Exception e) {
                    Log("Error while checking trades:\n" + ExceptionHelper.GetExceptionText(e, true, true));
                }
                StopEvent.Wait(UpdateInterval);
            }
        }

        private void CheckTrades(IEnumerable<OfferChecker> checkers)
        {            
            //http://api.steampowered.com/IEconService/GetTradeOffers/v1?key=XXXXXXXXXXXXXXXXXXXXXXXXXX&get_received_offers=1&active_only=1            
            OffersResponse offers = TradeWebApi.GetActiveTradeOffers(false, true, true);
            if (offers.TradeOffersReceived != null) {
                List<Offer> newOffers = offers.TradeOffersReceived.FindAll(o => o.TradeOfferState == TradeOfferState.TradeOfferStateActive && !ProcessedOffers.Contains(o.TradeOfferId));
                foreach (Offer o in newOffers) {
                    CheckOffer(o, offers.Descriptions, checkers);
                }
            }
        }
        

        private void CheckOffer(Offer o, List<AssetDescription> descriptions, IEnumerable<OfferChecker> checkers)
        {
            List<CEconAsset> toGive = o.ItemsToGive ?? new List<CEconAsset>();
            List<CEconAsset> toReceive = o.ItemsToReceive ?? new List<CEconAsset>();
            bool accept = checkers.Any(c => c.CheckOffer(o));
            Log(
                string.Format(
                    "Trade {0}\nWants:\n{1}\nOffers:\n{2}\n{3}\n",
                    o.TradeOfferId,
                    GetOfferText(toGive, descriptions),
                    GetOfferText(toReceive, descriptions),
                    accept ? "Accept" : "Ignore"
                )
            );
            
            if (accept) {
                //accept
                Log("Accepting " + o.TradeOfferId);
                TradeOffer to = null;
                if (OfferManager.GetOffer(o.TradeOfferId, out to)) {
                    TradeOfferAcceptResponse acceptResp = to.Accept();
                    if (acceptResp.Accepted) {
                        Log(o.TradeOfferId + " accepted");
                    } else { 
                        TradeOfferState curState = TradeWebApi.GetOfferState(o.TradeOfferId);
                        Log(string.Format("Can't accept {0}: {1}", o.TradeOfferId, acceptResp.TradeError ?? ""));
                        //do not add to processed, return and retry next time
                        //it seems there is Steam accept error for some reason
                        throw new TradeAcceptException(acceptResp.TradeError);
                    }
                } else {
                    Log("Offer " + o.TradeOfferId + " not found");
                }
            }
            ProcessedOffers.Add(o.TradeOfferId);
        }
        
        private void Log(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("MM\\/dd HH\\:mm\\:ss") + " " + message);
        }

        private string GetOfferText(List<CEconAsset> assets, List<AssetDescription> descriptions)
        {
            if (assets == null) {
                return "";
            }
            descriptions = descriptions ?? new List<AssetDescription>();

            var groups = assets
                .GroupBy(a => new { AppId = int.Parse(a.AppId), a.ClassId, a.InstanceId })
                .OrderByDescending(g => g.Count());

            StringBuilder sb = new StringBuilder();

            foreach (var g in groups) { 
                var key = g.Key;
                AssetDescription desc = descriptions.Find(d => d.AppId == key.AppId && d.ClassId == key.ClassId && d.InstanceId == key.InstanceId);
                sb.AppendFormat("{0,4} {1}\n", g.Count(), desc != null ? desc.MarketName : "Unknown");
            }
            return sb.ToString();
        }
    }
}
