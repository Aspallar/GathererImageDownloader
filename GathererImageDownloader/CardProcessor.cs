using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net;

namespace GathererImageDownloader
{
    class CardProcessor
    {
        private StreamWriter fileWriter; //For writing the Lua files line by line
        private WebClient imageDownloader = new WebClient(); //For downloading images
        private IList<JToken> mtgSets; //The list of magic sets we'll be using

        //List of sets we want to skip so we don't use their art or flavor text
        List<string> setsToSkip = new List<string> { "EXP" }; // EXP is the BFZ Expeditions set

        public bool DownloadImages { get; set; }

        /// <summary>
        /// Constructor of this class
        /// </summary>
        /// <param name="jsonFile">JSON file with MTG set data</param>
        public CardProcessor(string jsonFile)
        {
            CreateSetList(jsonFile);
            FilterSetList();
        }

        /// <summary>
        /// Create a list of MTG sets from the input JSON file
        /// </summary>
        /// <param name="jsonFile">JSON file with MTG set data</param>
        private void CreateSetList(string jsonFile)
        {
            JObject jobj = JObject.Parse(File.ReadAllText(jsonFile));
            mtgSets = jobj.Root.Children().ToList();
        }

        /// <summary>
        /// Removes unwanted sets from the set list
        /// </summary>
        private void FilterSetList()
        {
            foreach (var setCode in setsToSkip)
            {
                var sets = mtgSets.Where(s => s.Path == setCode);
                if (sets.Count() > 0)
                {
                    mtgSets.Remove(sets.First());
                }
            }
        }

        /// <summary>
        /// Turns an inputed list of card names into a lua file with all relevant information.
        /// Will also download the card's picture if needed.
        /// </summary>
        /// <param name="outputLuaFileName">Output lua filename</param>
        /// <param name="inputList">Input list filename</param>
        public void ProcessCardList(string outputLuaFileName, string inputList)
        {
            string[] CardsToUpload = File.ReadAllLines(inputList);
            fileWriter = new StreamWriter(outputLuaFileName);

            fileWriter.WriteLine("local data = {\r\n");

            foreach (string cardEntry in CardsToUpload)
            {
                ProcessCardEntry(cardEntry);
            }

            fileWriter.WriteLine("};\r\nreturn data");
            fileWriter.Close();
        }

        /// <summary>
        /// Processes a single entry from the input list
        /// </summary>
        /// <param name="cardEntry">One line from the input list</param>
        private void ProcessCardEntry(string cardEntry)
        {
            //First we split up the entry into its relevant parts
            string[] splitEntry = cardEntry.Split('$');
            string cardName = splitEntry[0]; // The name is expected to always be there

            JToken cardData = FindCardData(cardName);

            if (cardData != null) //If card data was found...
            {
                // Use the inputed override set code if one is present
                if (splitEntry.Length > 1)
                {
                    cardData["setcode"] = splitEntry[1];
                }

                //Download the image if needed and write the card's info to lua
                if (DownloadImages)
                    DownloadCardImage(cardData);
                WriteCardInfo(cardData);
            }
            else //Otherwise
            {
                Console.WriteLine($"ERROR: \"{cardName}\" couldn't be found. Typo?");
            }
        }

        /// <summary>
        /// Find a card's data object from the JSON source
        /// </summary>
        /// <param name="cardName">Name of the card to search for</param>
        /// <returns>That card's data object, if one was found</returns>
        private JToken FindCardData(string cardName)
        {
            JToken cardData = null; //This will remain null if no card is found
            foreach (JToken set in mtgSets.Reverse()) //We're searching in reverse order to get the most recent printings first
            {
                cardData = FindCardInSet(cardName, set); //Search the current set for the card

                if (cardData != null) // If the card was found, make some changes to it and end the loop
                {
                    cardData["name"] = FilterOutUnusableCharacters((string)cardData["name"]); // Cleaning up the card name
                    cardData["setcode"] = set.Path.ToString(); // Adding the setcode field to the card data manually
                    break;
                }
            }

            return cardData;
        }

        /// <summary>
        /// Search a set for a card
        /// </summary>
        /// <param name="cardName">Card name to look for</param>
        /// <param name="set">Card set to search</param>
        /// <returns>Found card data, if any</returns>
        private JToken FindCardInSet(string cardName, JToken set)
        {
            return (from card in set.First["cards"].Children() //SQL-style query for clarity
                    where (FilterOutUnusableCharacters((string)card["name"]) == cardName) && (card["multiverseid"] != null)
                    select card).FirstOrDefault();
        }

        /// <summary>
        /// Converts a card's JSON data to written LUA for use on the wiki
        /// </summary>
        /// <param name="cardData">JSON data describing a Magic card</param>
        private void WriteCardInfo(JToken cardData)
        {
            fileWriter.Write("{");
            WriteLuaString("Name", cardData["name"]);
            WriteLuaStringList("Names", cardData["names"]);
            fileWriter.WriteLine($"SetCode=\"{cardData["setcode"]}\";");
            WriteLuaString("Manacost", cardData["manaCost"]);
            WriteLuaInt("cmc", cardData["cmc"]);
            WriteLuaStringList("Colors", cardData["colors"]);
            WriteLuaString("Type", cardData["type"]);
            WriteLuaStringList("SuperTypes", cardData["supertypes"]);
            WriteLuaStringList("Types", cardData["types"]);
            WriteLuaStringList("SubTypes", cardData["subtypes"]);
            WriteLuaString("Rarity", cardData["rarity"]);
            WriteMultiLineLuaString("Text", cardData["text"]);
            WriteMultiLineLuaString("Flavor", cardData["flavor"]);
            WriteLuaString("Artist", cardData["artist"]);

            if (cardData["setcode"] == null || (string)cardData["number"] == null)
                throw new Exception("Card with no setcode and/or number");
            WriteLuaString("CardNumber", cardData["setcode"] + (string)cardData["number"]);

            WriteLuaString("Power", cardData["power"]);
            WriteLuaString("Toughness", cardData["toughness"]);
            WriteLuaInt("Loyalty", cardData["loyalty"]);
            WriteLuaInt("MultiverseID", cardData["multiverseid"]);
            WriteLuaString("Watermark", cardData["watermark"]);
            WriteRulings(cardData["rulings"]);
            fileWriter.WriteLine("};\r\n");
        }

        /// <summary>
        /// Downloads the card's image from Gatherer if it's not already in the image output folder
        /// </summary>
        /// <param name="cardData">Card data object</param>
        private void DownloadCardImage(JToken cardData)
        {
            string cardName = (string)cardData["name"];
            string filename = $@"Images\{cardName}.png";
            if (!File.Exists(filename))
            {
                Console.WriteLine($"Downloading image for \"{cardName}\"");
                imageDownloader.DownloadFile($"http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid={cardData["multiverseid"]}&type=card", filename);
            }
        }

        private void WriteLuaString(string name, string text)
        {
            fileWriter.WriteLine($"{name}=\"{CleanupWikitext(text)}\";");
        }

        private void WriteLuaString(string name, JToken element)
        {
            if (element == null) { return; }

            fileWriter.WriteLine($"{name}=\"{CleanupWikitext((string)element)}\";");
        }

        private void WriteMultiLineLuaString(string name, JToken element)
        {
            if (element == null) { return; }

            fileWriter.WriteLine($"{name}=[=[{CleanupWikitext((string)element)}]=];");
        }

        private void WriteLuaStringList(string name, IEnumerable<JToken> elements)
        {
            if ((elements == null) || (elements.ToList().Count <= 0)) { return; }

            fileWriter.Write(name + "={");
            foreach (JToken item in elements)
            {
                fileWriter.Write($"\"{item}\"{(elements.Count() == 1 ? "" : ";")}");
            }
            fileWriter.WriteLine("};");
        }

        private void WriteLuaInt(string name, JToken element)
        {
            if (element == null) { return; }

            fileWriter.WriteLine($"{name}={element};");
        }

        private void WriteRulings(IEnumerable<JToken> rulings)
        {
            if ((rulings == null) || (rulings.ToList().Count <= 0)) { return; }

            fileWriter.WriteLine("Rulings={");
            foreach (JToken ruling in rulings)
            {
                fileWriter.Write($"\t{{Date=\"{ruling["date"]}\";");
                fileWriter.WriteLine($"Text=[=[{ruling["text"]}]=];}};");
            }
            fileWriter.WriteLine("};");
        }

        //We can't expect people to use "Æ" on the wiki, so no card names should use it.
        private string FilterOutUnusableCharacters(string input)
        {
            return input.Replace("Æ", "Ae");
        }

        //This turns every insteance of magic markup into a template. T is already a template so we're using Tap instead. Finally we're using line breaks the wiki will process properly.
        private string CleanupWikitext(string input)
        {
            return FilterOutUnusableCharacters(input.Replace("{", "{{").Replace("}", "}}").Replace("{{T}}", "{{Tap}}").Replace("\n", "<br/>\n"));
        }
    }
}
