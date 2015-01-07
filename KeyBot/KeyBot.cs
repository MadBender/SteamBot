using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        private SteamClient SteamClient;
        private SteamTrading SteamTrade;
        private SteamUser SteamUser;
        private CallbackManager CallbackManager;        

        private string TwoFactorAuth;
        private string AuthCode;
        private bool RetryLogin;

        private string UniqueID;
        private string SessionID;
        private string Token;
        private string TokenSecure;
        private string UserNonce;

        private TradeOfferManager OfferManager;
        private HashSet<string> ProcessedOffers;

        //bot job
        private ManualResetEventSlim StopEvent;
        private ManualResetEventSlim StoppedEvent;      

        public KeyBot(string login, string password, string apiKey)
        {
            Login = login;
            Password = password;
            ApiKey = apiKey;                                   
        }

        public void Start()
        {
            new Thread(() => {
                SteamClient = new SteamClient();                
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

                RetryLogin = true;
                StopEvent = new ManualResetEventSlim();
                SteamClient.Connect();
                while (!StopEvent.IsSet) {
                    // in order for the callbacks to get routed, they need to be handled by the manager
                    CallbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
                }
            }).Start();
        }

        public void Stop()
        {
            RetryLogin = false;
            StopInternal();
        }

        private void StopInternal()
        {
            StopEvent.Set();
            if (StoppedEvent != null) {
                StoppedEvent.Wait();
            }
            SteamClient.Disconnect();
        }

        #region Logon
        private void OnConnected(SteamClient.ConnectedCallback callback)
        {            
            if (callback.Result != EResult.OK) {
                Log("Unable to connect to Steam: " + callback.Result);
                StopEvent.Set();
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
            StopInternal();
            if (RetryLogin) {
                // after recieving an AccountLogonDenied, we'll be disconnected from steam
                // so after we read an authcode from the user, we need to reconnect to begin the logon flow again
                Thread.Sleep(5000);
                Log("Reconnecting");
                Start();
            }
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
                StopEvent.Set();
                return;
            }

            Log("Successfully logged on!");
            UserNonce = callback.WebAPIUserNonce;

            // at this point, we'd be able to perform actions on Steam            
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            RetryLogin = callback.Result != EResult.LogonSessionReplaced;
            Log("Logged off of Steam: " + callback.Result);
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
            bool authd = SteamWeb.Authenticate(UniqueID, SteamClient, out SessionID, out Token, out TokenSecure, UserNonce);
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
            StoppedEvent = new ManualResetEventSlim();
            ProcessedOffers = new HashSet<string>();
            OfferManager = new TradeOfferManager(ApiKey, SessionID, Token, TokenSecure);                
            while (!StopEvent.IsSet) {
                try {
                    //Log("Checking trades");
                    CheckTrades();
                }
                catch (Exception e) {
                    Log("Error while checking trades: " + e.Message);
                }
                StopEvent.Wait(10000);
            }            
            StoppedEvent.Set();            
        }

        public void CheckTrades()
        {           
            //http://api.steampowered.com/IEconService/GetTradeOffers/v1?key=XXXXXXXXXXXXXXXXXXXXXXXXXX&get_received_offers=1&active_only=1            
            OffersResponse offers = new TradeOfferWebAPI(ApiKey).GetActiveTradeOffers(false, true, false);
            List<Offer> newOffers = offers.TradeOffersReceived.FindAll(o => o.TradeOfferState == TradeOfferState.TradeOfferStateActive && !ProcessedOffers.Contains(o.TradeOfferId));            
            foreach(Offer o in newOffers) {
                CheckOffer(o);
            }
        }

        private void CheckOffer(Offer o)
        {
            int myKeyCount = o.ItemsToGive.Count(IsKey);
            int myOtherCount = o.ItemsToGive.Count - myKeyCount;
            int theirKeyCount = o.ItemsToReceive.Count(IsKey);
            int theirOtherCount = o.ItemsToReceive.Count - theirKeyCount;

            bool accept = theirKeyCount >= myKeyCount && myOtherCount == 0 && theirOtherCount > 0;

            Log(
                string.Format(
                    "Trade {0}: wants {1} keys, {2} others; offers {3} keys, {4} others. {5}",
                    o.TradeOfferId,
                    myKeyCount,
                    myOtherCount,
                    theirKeyCount,
                    theirOtherCount,
                    accept ? "Accept" : "Ignore"
                )
            );
            if (accept) {
                //accept
                Log("Accepting " + o.TradeOfferId);
                TradeOffer to = null;
                if (OfferManager.GetOffer(o.TradeOfferId, out to)) {
                    to.Accept();
                    Log(o.TradeOfferId + " accepted");
                } else {
                    Log("Can't accept " + o.TradeOfferId);
                }
            }
            ProcessedOffers.Add(o.TradeOfferId);
        }

        private static List<string> KeyClassIDs = new List<string>{
            "360447207", //Phoenix key
            "319543459", //CSGO key
            "613613001", //Breakout key
            "506857900", //Huntsman key
            "319542879", //ESports key
            "319540568", //Winter Offensive key
            "638240119"  //Vanguard key
        };

        private bool IsKey(CEconAsset asset)
        {
            return KeyClassIDs.Contains(asset.ClassId) && asset.InstanceId == "143865972" && asset.AppId == "730";
        }

        private void Log(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("MM/dd HH:mm:ss") + " " + message);
        }

    }
}
