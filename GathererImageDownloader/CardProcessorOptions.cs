namespace GathererImageDownloader
{
    class CardProcessorOptions
    {
        public bool DownloadImages { get; set; }
        public bool Verbose { get; set; }
        public bool UseSpecifiedSet { get; set; }

        public CardProcessorOptions()
        {
            DownloadImages = true;
            Verbose = false;
            UseSpecifiedSet = true;
        }

        public bool Parse(string[] args)
        {
            foreach (string arg in args)
            {
                switch (arg.ToLowerInvariant())
                {
                    case "--noimages":
                        DownloadImages = false;
                        break;
                    case "--verbose":
                        Verbose = true;
                        break;
                    case "--allsets":
                        UseSpecifiedSet = false;
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }
    }
}
