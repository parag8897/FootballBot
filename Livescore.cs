namespace BotBuilder.Samples.AdaptiveCards
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Livescore
    {
        public string Name { get; set; }

        public int Rating { get; set; }

        public int NumberOfReviews { get; set; }

        public int PriceStarting { get; set; }

        public string Image { get; set; }

        public IEnumerable<string> MoreImages { get; set; }

        public string Location { get; set; }

        public string Team1 { get; set; }

        public string Team2 { get; set; }

        public string lscore { get; set; }


    }
}