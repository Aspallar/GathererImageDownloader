using System;

namespace GathererImageDownloader
{
    class Program
    {
        //This is where the program starts when it is executed
        static void Main(string[] args)
        {
            bool downloadImages;
            if (!ProcessCommandLine(args, out downloadImages))
                return;

            Console.WriteLine("Loading JSON data...");
            CardProcessor cardImporter = new CardProcessor("Data/AllSets-x.json");
            cardImporter.DownloadImages = downloadImages;

            Console.WriteLine("Processing main card list");
            cardImporter.ProcessCardList("Lua/Cards.lua", "Data/CardsToUpload.txt");

            Console.WriteLine("Processing secondary card list");
            cardImporter.ProcessCardList("Lua/OtherCards.lua", "Data/OtherCardsToUpload.txt");

            Console.WriteLine("Process finished. Press a key to exit.");
            Console.ReadKey();
        }

        private static bool ProcessCommandLine(string[] args, out bool downloadImages)
        {
            downloadImages = true;
            if (args.Length == 1)
            {
                if (args[0].ToLowerInvariant() == "--noimages")
                {
                    downloadImages = false;
                }
                else
                {
                    ShowUsage();
                    return false;
                }
            }
            else if (args.Length != 0)
            {
                ShowUsage();
                return false;
            }
            return true;
        }

        private static void ShowUsage()
        {
            Console.WriteLine("GathererImageDownloader [--noimages]");
        }
    }
}
