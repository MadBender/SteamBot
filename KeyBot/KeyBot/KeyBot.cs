using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2;

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
        private bool IsRunning;

        private string TwoFactorAuth;
        private string AuthCode;

        public KeyBot(string login, string password, string apiKey)
        {
            Login = login;
            Password = password;
            ApiKey = apiKey;
            SteamClient = new SteamClient();
           // SteamClient.AddHandler(new SteamNotificationHandler());
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
        }

        public void Start()
        {
            SteamClient.Connect();
            IsRunning = true;
            while (IsRunning) {
                // in order for the callbacks to get routed, they need to be handled by the manager
                CallbackManager.RunWaitCallbacks();
            }
        }

        public void Stop()
        {
            SteamClient.Disconnect();
        }


        private void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK) {
                Console.WriteLine("Unable to connect to Steam: {0}", callback.Result);

                IsRunning = false;
                return;
            }

            Console.WriteLine("Connected to Steam! Logging in '{0}'...", Login);

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
            // after recieving an AccountLogonDenied, we'll be disconnected from steam
            // so after we read an authcode from the user, we need to reconnect to begin the logon flow again

            Console.WriteLine("Disconnected from Steam, reconnecting...");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            SteamClient.Connect();
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLogonDeniedNeedTwoFactorCode;

            if (isSteamGuard || is2FA) {
                Console.WriteLine("This account is SteamGuard protected!");

                if (is2FA) {
                    Console.Write("Please enter your 2 factor auth code from your authenticator app: ");
                    TwoFactorAuth = Console.ReadLine();
                } else {
                    Console.Write("Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);
                    AuthCode = Console.ReadLine();
                }
                return;
            }

            if (callback.Result != EResult.OK) {
                Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);
                IsRunning = false;
                return;
            }

            Console.WriteLine("Successfully logged on!");

            // at this point, we'd be able to perform actions on Steam
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);
        }

        private void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Updating sentryfile...");

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

            Console.WriteLine("Done!");
        }

    }
}
