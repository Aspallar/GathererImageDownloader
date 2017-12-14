using System;
using System.IO;
using System.Reflection;

namespace GathererImageDownloader
{
    class Program
    {
        private const string otherCardsFilename = "Data/OtherCardsToUpload.txt";
        private const string mainCardsFilename = "Data/CardsToUpload.txt";
        private const string cardDataFilename = "Data/AllSets-x.json";

        //This is where the program starts when it is executed
        static void Main(string[] args)
        {
            Console.WriteLine("GathererImageDownloader Version " + GetVersionString());

            var options = new CardProcessorOptions();
            if (!options.Parse(args))
            {
                ShowUsage();
                Pause();
                return;
            }

            CardProcessor cardImporter;
            if (File.Exists(cardDataFilename))
            {
                Console.WriteLine("Loading JSON data...");
                cardImporter = new CardProcessor(cardDataFilename, options);
            }
            else
            {
                Console.WriteLine($"ERROR: MTG card data file \"{cardDataFilename}\" is missing.");
                return;
            }

            if (File.Exists(mainCardsFilename))
            {
                Console.WriteLine("Processing main card list");
                cardImporter.ProcessCardList("Lua/Cards.lua", mainCardsFilename);
            }
            else
            {
                Console.WriteLine($"\"{otherCardsFilename}\" not found. Skipping main card list.");
            }

            if (File.Exists(otherCardsFilename))
            {
                Console.WriteLine("Processing secondary card list");
                cardImporter.ProcessCardList("Lua/OtherCards.lua", otherCardsFilename);
            }
            else
            {
                Console.WriteLine($"\"{otherCardsFilename}\" not found. Skipping secondary card list.");
            }

            Console.WriteLine("Process finished.");
            Pause();
        }

        private static void ShowUsage()
        {
            Console.WriteLine("\nUsage:\n");
            Console.WriteLine("GathererImageDownloader [[--noimages] [--verbose] [--allsets]]\n");
            Console.WriteLine("--noimages = don't download images");
            Console.WriteLine("--verbose  = more information displayed during processing");
            Console.WriteLine("--allsets  = use \"old\" method to find cards");
            Console.WriteLine("             search all sets for match, not just one specified after the $");
            Console.WriteLine("             starting with the last set in data file and working backwards.\n");
        }

        private static void Pause()
        {
            if (!Console.IsOutputRedirected)
            {
                Console.WriteLine("Press a key to exit.");
                Console.ReadKey();
            }
        }

        private static string GetVersionString()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
