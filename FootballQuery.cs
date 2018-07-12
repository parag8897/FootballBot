namespace BotBuilder.Samples.AdaptiveCards
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class FootballQuery
    {
        [Required]
        public string Team1 { get; set; }

        [Required]
        public string  Team2 { get; set; }

        //[Range(1, 60)]
        //public int? Nights { get; set; }

        public static FootballQuery Parse(dynamic o)
        {
            try
            {
                return new FootballQuery
                {
                    Team1 = o.Team1.ToString(),
                    Team2 = o.Team2.ToString(),
                    //Nights = int.Parse(o.Nights.ToString())
                };
            }
            catch
            {
                throw new InvalidCastException("Query could not be read");
            }
        }
    }
}