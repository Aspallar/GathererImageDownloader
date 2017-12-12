using System;

namespace GathererImageDownloader
{
    class Program
    {
        //This is where the program starts when it is executed
        static void Main(string[] args)
        {
            Console.WriteLine("Loading JSON data...");
            CardProcessor cardImporter = new CardProcessor("Data/AllSets-x.json");

            Console.WriteLine("Processing main card list");
            cardImporter.ProcessCardList("Lua/Cards.lua", "Data/CardsToUpload.txt");

            Console.WriteLine("Processing secondary card list");
            cardImporter.ProcessCardList("Lua/OtherCards.lua", "Data/OtherCardsToUpload.txt");

            Console.WriteLine("Process finished. Press a key to exit.");
            Console.ReadKey();
        }

    }
}
