using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using AutoMapper;
using KeyBot.Log;
using KeyBot.Models;
using KeyBot.OfferValidators;
using KeyBot.Properties;
using Newtonsoft.Json;
using SteamKit2;
using SteamTrade.TradeOffer;

namespace KeyBot
{
    internal static class Program
    {
        public static string CurrentDirectory;
        private static ILogger Log;
        private static KeyBot Bot;

        static void Main(string[] args)
        {
            Log = new NLogLogger(NLog.LogManager.GetLogger("ApplicationLog"));
            InitAutomapper();
            Console.OutputEncoding = Encoding.Unicode;
     
            CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var settings = Settings.Default;
            // Hacking around https
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            Thread botThread = new Thread(() => {
                //this is passed between bots and collects all logon info
                var logonDetails = new SteamUser.LogOnDetails {
                    Username = settings.Login,
                    Password = settings.Password
                };
                while (true) {
                    if (logonDetails.LoginKey != null) {
                        logonDetails.Password = null;
                    }
                    Log.Log(LogLevel.Debug, "Starting the bot");
                    Bot = new KeyBot(
                        logonDetails,
                        settings.ApiKey, 
                        settings.UpdateInterval, 
                        new List<OfferValidator>{ 
                            new RuleSwapOfferValidator(JsonConvert
                                .DeserializeObject<CompactValidationRuleSet>(File.ReadAllText(Path.Combine(Program.CurrentDirectory, "SwapRules.json")))
                                .GetFullRuleSet()
                            )                
                        },
                        Log
                    );
                    Bot.Start();
                    Bot.Wait();
                    if (Bot.LogoffReason == EResult.LogonSessionReplaced) {
                        Log.Log(LogLevel.Warning, "Session replaced, no more bots will be created");
                        return;
                    }

                    //if need additional auth, retry immediately before it expires
                    if (!Bot.IsAdditionalAuthCode(Bot.LogoffReason)) {
                        //else reset auth codes and wait
                        logonDetails.AuthCode = null;
                        logonDetails.TwoFactorCode = null;
                        Thread.Sleep(10000);
                    } else {
                        Thread.Sleep(1000);
                    }
                }
            });
            botThread.Start();
            botThread.Join();

            //exiting
            Console.WriteLine("Press any key");
            Console.ReadKey(true);
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

        private static void InitAutomapper()
        {
            Mapper.CreateMap<CEconAsset, CEconAssetModel>();
        }
    }
}
