namespace BotBuilder.Samples.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Connector;
    using Newtonsoft.Json.Linq;

    [Serializable]
    public class FootballDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            var message = context.Activity as IMessageActivity;
            var query = FootballQuery.Parse(message.Value);

            await context.PostAsync($"Ok. Searching for the League Matches Between   {query.Team1} and {query.Team2} ");

            try
            {
                await SearchMatches(context, query);
            }
            catch (FormCanceledException ex)
            {
                await context.PostAsync($"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}");
            }
        }

        private async Task SearchMatches(IDialogContext context, FootballQuery searchQuery)
        {
            var Football = this.GetMatches(searchQuery);

            // Result count
            var title = $"I found in total {Football.Count()} matches for your teams:";
            var intro = new List<CardElement>()
            {
                    new TextBlock()
                    {
                        Text = title,
                        Size = TextSize.ExtraLarge,
                        Speak = $"<s>{title}</s>"
                    }
            };

            // Hotels in rows of three
            var rows = Split(Football, 3)
                .Select(group => new ColumnSet()
                {
                    Columns = new List<Column>(group.Select(AsFootballItem))
                });

            var card = new AdaptiveCard()
            {
                Body = intro.Union(rows).ToList()
            };

            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            var reply = context.MakeMessage();
            reply.Attachments.Add(attachment);

            await context.PostAsync(reply);
        }

        private Column AsFootballItem(Football match)
        {
            var submitActionData = JObject.Parse("{ \"Type\": \"MatchSelection\" }");
            submitActionData.Merge(JObject.FromObject(match));

            return new Column()
            {
                Size = "20",
                Items = new List<CardElement>()
                {
                    new TextBlock()
                    {
                        Text = match.Name,
                        Speak = $"<s>{match.Name}</s>",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Wrap = false,
                        Weight = TextWeight.Bolder
                    },
                    new Image()
                    {
                        Size = ImageSize.Auto,
                        Url = match.Image
                    }
                },
                SelectAction = new SubmitAction()
                {
                    DataJson = submitActionData.ToString()
                }
            };
        }

        private IEnumerable<Football> GetMatches(FootballQuery searchQuery)
        {
            var matches= new List<Football>();

            // Filling the hotels results manually just for demo purposes
            for (int i = 1; i <= 2; i++)
            {
                if (i == 1)
                {
                    Football match = new Football()
                    {
                        Name = $"Match No. {i}",
                        Location = searchQuery.Team1,
                        // Rating = random.Next(1, 5),
                        // NumberOfReviews = random.Next(0, 5000),
                        // PriceStarting = random.Next(80, 450),
                        Image = $"https://www.games4reloaded.com/wp-content/uploads/2018/07/prediction-800x445.jpg",
                        MoreImages = new List<string>()
                    {
                        "https://nesncom.files.wordpress.com/2018/07/samuel-umtiti.jpg?w=640",
                        "https://media.fox4news.com/media.fox4news.com/photo/2018/07/09/France%20vs%20Belgium_1531173430360.jpg_5771874_ver1.0_640_360.jpg",
                        "https://images.indianexpress.com/2018/07/fifa-ap-m.jpg",
                        "https://cdn.images.dailystar.co.uk/dynamic/1/photos/437000/France-vs-Belgium-World-Cup-Russia-semi-final-girls-1389437.jpg"
                    }
                    };

                    matches.Add(match);
                }
                else
                {
                    Football match = new Football()
                    {
                        Name = $"Match No. {i}",
                        Location = searchQuery.Team2,
                        // Rating = random.Next(1, 5),
                        // NumberOfReviews = random.Next(0, 5000),
                        // PriceStarting = random.Next(80, 450),
                        Image = $"http://sportfunlive.com/wp-content/uploads/2018/07/croatia-vs-england.jpg",
                        MoreImages = new List<string>()
                    {
                        "https://pickssoccer.com/wp-content/uploads/2018/07/England-vs-Croatia-min.jpg",
                        "https://media.fox4news.com/media.fox4news.com/photo/2018/07/09/France%20vs%20Belgium_1531173430360.jpg_5771874_ver1.0_640_360.jpg",
                        "https://i2-prod.mirror.co.uk/incoming/article12879927.ece/ALTERNATES/s482b/FBL-WC-2018-MATCH30-ENG-PAN.jpg",
                        "https://static.standard.co.uk/s3fs-public/thumbnails/image/2018/07/10/10/2018-07-07T161833Z-1348922517-RC1BAB4773E0-RTRMADP-3-SOCCER-WORLDCUP-SWE-ENG-FANS.JPG?w968h681"
                    }
                    };

                    matches.Add(match);
                }
            }

            //matches.Sort((h1, h2) => h1.PriceStarting.CompareTo(h2.PriceStarting));

            return matches;
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(IEnumerable<T> list, int parts)
        {
            return list.Select((item, ix) => new { ix, item })
                       .GroupBy(x => x.ix % parts)
                       .Select(x => x.Select(y => y.item));
        }
    }
}