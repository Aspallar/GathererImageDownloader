using System;

namespace GathererImageDownloader
{
    class Program
    {
        //This is where the program starts when it is executed
        static void Main(string[] args)
        {
            bool downloadImages;
            bool verbose;
            if (!ProcessCommandLine(args, out downloadImages, out verbose))
            {
                ShowUsage();
                return;
            }

            Console.WriteLine("Loading JSON data...");
            CardProcessor cardImporter = new CardProcessor("Data/AllSets-x.json")
            {
                DownloadImages = downloadImages,
                Verbose = verbose,
            };

            Console.WriteLine("Processing main card list");
            cardImporter.ProcessCardList("Lua/Cards.lua", "Data/CardsToUpload.txt");

            Console.WriteLine("Processing secondary card list");
            cardImporter.ProcessCardList("Lua/OtherCards.lua", "Data/OtherCardsToUpload.txt");

            Console.WriteLine("Process finished. Press a key to exit.");
            Console.ReadKey();
        }


        private static bool ProcessCommandLine(string[] args, out bool downloadImages, out bool verbose)
        {
            downloadImages = true;
            verbose = false;
            foreach (string arg in args)
            {
                switch (arg.ToLowerInvariant())
                {
                    case "--noimages":
                        downloadImages = false;
                        break;
                    case "--verbose":
                        verbose = true;
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }

        private static void ShowUsage()
        {
            Console.WriteLine("GathererImageDownloader [--noimages] [--verbose]");
        }
    }
}
