namespace BotBuilder.Samples.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using global::AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Connector;
    using Newtonsoft.Json.Linq;

    [Serializable]
    public class LivescoreDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            var message = context.Activity as IMessageActivity;
            var query = FootballQuery.Parse(message.Value);

            await context.PostAsync($"Ok. Searching for the LiveScore Between   {query.Team1} and {query.Team2} ");

            try
            {
                await SearchLivescore(context, query);
            }
            catch (FormCanceledException ex)
            {
                await context.PostAsync($"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}");
            }
        }

        private async Task SearchLivescore(IDialogContext context, FootballQuery searchQuery)
        {
            var Livescore= this.GetLivescore(searchQuery);

            // Result count
            var title = $"I found Livescore between your teams your teams:";
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
            var rows = Split(Livescore, 2)
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

        private Column AsFootballItem(Livescore match)
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

        private IEnumerable<Livescore> GetLivescore(FootballQuery searchQuery )
        {
            var livescore = new List<Livescore>();
            for (int i = 1; i <= 2; i++)
            {
                if (i == 1)
                {
                    Livescore match = new Livescore()
                    {
                        Name = $"Match No. {i}",
                        Team1 = searchQuery.Team1,
                        Team2 = searchQuery.Team2,
                        lscore= "https://apifootball.com/api/?action=get_events&from=2016-10-30&to=2016-11-01&league_id=62&APIkey=ac41d66e732ff9ef06a1e697e79a039bbb971bf7f093014e9b200c0003bdbd63",
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
                    livescore.Add(match);
                }
                else
                {
                    Livescore match = new Livescore()
                    {
                        Name = $"Match No. {i}",
                        Team1 = searchQuery.Team1,
                        Team2 = searchQuery.Team2,
                        lscore = "https://apifootball.com/api/?action=get_events&from=2016-10-30&to=2016-11-01&league_id=62&APIkey=ac41d66e732ff9ef06a1e697e79a039bbb971bf7f093014e9b200c0003bdbd63",
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
                    livescore.Add(match);
                }
                
            }
            return livescore;
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(IEnumerable<T> list, int parts)
        {
            return list.Select((item, ix) => new { ix, item })
                       .GroupBy(x => x.ix % parts)
                       .Select(x => x.Select(y => y.item));
        }
    }
}