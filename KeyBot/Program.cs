using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using KeyBot.Properties;

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
            Bot = new KeyBot(settings.Login, settings.Password, settings.ApiKey, settings.UpdateInterval);
            Bot.Start();
            while (true) {
                HandleCommand(Console.ReadLine());
            }
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
