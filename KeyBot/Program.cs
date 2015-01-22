using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using KeyBot.Properties;
using SteamKit2;

namespace KeyBot
{
    internal static class Program
    {
        public static string CurrentDirectory;

        private static KeyBot Bot;

        static void Main(string[] args)
        {
            CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var settings = Settings.Default;
            // Hacking around https
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            Thread botThread = new Thread(() => {
                string authCode = null;
                string twoFactorAuth = null;
                while (true) {
                    Console.WriteLine("\nStarting the bot");
                    Bot = new KeyBot(settings.Login, settings.Password, settings.ApiKey, settings.UpdateInterval) { 
                        AuthCode = authCode,
                        TwoFactorAuth = twoFactorAuth
                    };
                    Bot.Start();
                    Bot.Wait();
                    if (Bot.LogoffReason == EResult.LogonSessionReplaced) {
                        Console.WriteLine("No more bots will be created");
                        return;
                    }
                    authCode = Bot.AuthCode;
                    twoFactorAuth = Bot.TwoFactorAuth;
                    Thread.Sleep(10000);
                }
            });
            botThread.Start();
            botThread.Join();
            /*while (true) {
                HandleCommand(Console.ReadLine());
            }*/
        }

        private static void HandleCommand(string command)
        {
            switch (command) { 
                case "logoff":
                    Bot.Logoff();
                    break;
                default:
                    Console.WriteLine("Unknown command");
                    break;
            }
        }
    }
}
