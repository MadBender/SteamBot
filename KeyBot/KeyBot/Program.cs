using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using KeyBot.Properties;

namespace KeyBot
{
    internal static class Program
    {
        public static string CurrentDirectory;

        static void Main(string[] args)
        {
            CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var settings = Settings.Default;
            // Hacking around https
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            KeyBot bot = new KeyBot(settings.Login, settings.Password, settings.ApiKey);
            bot.Start();
            Console.WriteLine("Press any key");
            Console.ReadKey(true);
        }
    }
}
