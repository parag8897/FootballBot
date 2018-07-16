namespace BotBuilder.Samples.AdaptiveCards
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Newtonsoft.Json;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string LivescoreOption = "Livescore";

        private const string LeaguesOption = "Leagues";

        public async Task StartAsync(IDialogContext context)
        {
            //await context.PostAsync("Hi....There!");
            context.Wait(this.MessageReceivedAsync);
        }


        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Value != null)
            {
                // Got an Action Submit
                dynamic value = message.Value;
                string submitType = value.Type.ToString();
                switch (submitType)
                {
                    case "leagueSearch":
                        FootballQuery query1;
                        try
                        {
                            query1 = FootballQuery.Parse(value);

                            // Trigger validation using Data Annotations attributes from the FootballsQuery model
                            List<ValidationResult> results = new List<ValidationResult>();
                            bool valid = Validator.TryValidateObject(query1, new ValidationContext(query1, null, null), results, true);
                            if (!valid)
                            {
                                // Some field in the Football Query are not valid
                                var errors = string.Join("\n", results.Select(o => " - " + o.ErrorMessage));
                                await context.PostAsync("Please complete all the search parameters:\n" + errors);
                                return;
                            }
                        }
                        catch (InvalidCastException)
                        {
                            // Football Query could not be parsed
                            await context.PostAsync("Please complete all the search parameters");
                            return;
                        }

                        // Proceed with Footballs search
                        await context.Forward(new LeagueDialog(), this.ResumeAfterOptionDialog, message, CancellationToken.None);

                        return;

                    case "leagueSelection":
                        await SendFootballSelectionAsync(context, (League)JsonConvert.DeserializeObject<League>(value.ToString()));
                        context.Wait(MessageReceivedAsync);

                        return;

                    case "LivescoreSearch":
                        FootballQuery query2;
                        try
                        {
                            query2 = FootballQuery.Parse(value);

                            // Trigger validation using Data Annotations attributes from the FootballsQuery model
                            List<ValidationResult> results = new List<ValidationResult>();
                            bool valid = Validator.TryValidateObject(query2, new ValidationContext(query2, null, null), results, true);
                            if (!valid)
                            {
                                // Some field in the Football Query are not valid
                                var errors = string.Join("\n", results.Select(o => " - " + o.ErrorMessage));
                                await context.PostAsync("Please complete all the search parameters:\n" + errors);
                                return;
                            }
                        }
                        catch (InvalidCastException)
                        {
                            // Football Query could not be parsed
                            await context.PostAsync("Please complete all the search parameters");
                            return;
                        }

                        // Proceed with Livescore search
                        await context.Forward(new LivescoreDialog(), this.ResumeAfterOptionDialog, message, CancellationToken.None);

                        return;



                }
            }

            if (message.Text != null && (message.Text.ToLower().Contains("help") || message.Text.ToLower().Contains("support") || message.Text.ToLower().Contains("problem")))
            {
                await context.Forward(new SupportDialog(), this.ResumeAfterSupportDialog, message, CancellationToken.None);
            }
            else
            {
                await ShowOptionsAsync(context);
            }
        }


        private async Task ShowOptionsAsync(IDialogContext context)
        {
            AdaptiveCard card = new AdaptiveCard()
            {
                Body = new List<CardElement>()
                {
                    new Container()
                    {
                        Speak = "<s>Hello!</s><s>Are you looking for a LiveScore or a Leagues?</s>",
                        Items = new List<CardElement>()
                        {
                            new ColumnSet()
                            {
                                Columns = new List<Column>()
                                {
                                    new Column()
                                    {
                                        Size = ColumnSize.Auto,
                                        Items = new List<CardElement>()
                                        {
                                            new Image()
                                            {
                                                Url = "http://1000logos.net/wp-content/uploads/2017/01/fifa-emblem.jpg",
                                                Size = ImageSize.Medium,
                                                Style = ImageStyle.Person
                                            }
                                        }
                                    },
                                    new Column()
                                    {
                                        Size = ColumnSize.Stretch,
                                        Items = new List<CardElement>()
                                        {
                                            new TextBlock()
                                            {
                                                Text =  "Hello!",
                                                Weight = TextWeight.Bolder,
                                                IsSubtle = true
                                            },
                                            new TextBlock()
                                            {
                                                Text = "Are you looking for a LiveScore or Leagues?",
                                                Wrap = true
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                // Buttons
                Actions = new List<ActionBase>() {
                    new ShowCardAction()
                    {
                        Title = "Leagues",
                        Speak = "<s>Leagues</s>",
                        Card = GetFootballSearchCard()
                    },
                    new ShowCardAction()
                    {
                        Title = "Livescore",
                        Speak = "<s>Livescore</s>",
                        //Card = new AdaptiveCard()
                        //{
                        //    Body = new List<CardElement>()
                        //    {
                        //        new TextBlock()
                        //        {
                        //            Text = "Flights is not implemented =(",
                        //            Speak = "<s>Flights is not implemented</s>",
                        //            Weight = TextWeight.Bolder
                        //        }
                        //    }
                        //}
                        Card=GetFootballLivescoreCard()

                    }
                }
            };

            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            var reply = context.MakeMessage();
            reply.Attachments.Add(attachment);

            await context.PostAsync(reply, CancellationToken.None);

            context.Wait(MessageReceivedAsync);
        }


        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(this.MessageReceivedAsync);
        }


        private async Task ResumeAfterSupportDialog(IDialogContext context, IAwaitable<int> result)
        {
            var ticketNumber = await result;

            await context.PostAsync($"Thanks for contacting our support team. Your ticket number is {ticketNumber}.");
            context.Wait(this.MessageReceivedAsync);
        }


        private static AdaptiveCard GetFootballSearchCard()
        {
            return new AdaptiveCard()
            {
                Body = new List<CardElement>()
                {
                        // Footballs Search form
                        new TextBlock()
                        {
                            Text = "Welcome to the Football Carnival!",
                            Speak = "<s>Welcome to the Football Carnival!</s>",
                            Weight = TextWeight.Bolder,
                            Size = TextSize.Large
                        },
                        new TextBlock() { Text = "Please enter your team 1:" },
                        new TextInput()
                        {
                            Id = "Team1",
                            Speak = "<s>Please enter your team 1</s>",
                            Placeholder = "France, Brasil,etc",
                            Style = TextInputStyle.Text
                        },
                        new TextBlock() { Text = "Please enter your team 2:" },
                        new TextInput()
                        {
                            Id = "Team2",
                            Speak = "<s>Please enter your team 2</s>",
                            Placeholder = "France, Brasil,etc",
                            Style = TextInputStyle.Text
                        },
                        //new TextBlock() { Text = "How many nights do you want to stay?" },
                        //new NumberInput()
                        //{
                        //    Id = "Nights",
                        //    Min = 1,
                        //    Max = 60,
                        //    Speak = "<s>How many nights do you want to stay?</s>"
                        //}
                },
                Actions = new List<ActionBase>()
                {
                    new SubmitAction()
                    {
                        Title = "Search",
                        Speak = "<s>Search</s>",
                        DataJson = "{ \"Type\": \"leagueSearch\" }"
                    }
                }
            };
        }

     
        private static async Task SendFootballSelectionAsync(IDialogContext context, League league)
        {
           // var description = $"{Football.Rating} start with {Football.NumberOfReviews}. From ${Football.PriceStarting} per night.";
            var card = new AdaptiveCard()
            {
                Body = new List<CardElement>()
                {
                    new Container()
                    {
                        Items = new List<CardElement>()
                        {
                            new TextBlock()
                            {
                                Text = $"{league.Name} :: {league.Team1} vs {league.Team2}",
                                Weight = TextWeight.Bolder,
                                Speak = $"<s>{league.Name}</s>"
                            },
                            new TextBlock()
                            {
                                Text = "Incredible",
                                Speak = $"<s>Incredible</s>"
                            },
                            new Image()
                            {
                                Size = ImageSize.Large,
                                Url = league.Image
                            },
                            new ImageSet()
                            {
                                ImageSize = ImageSize.Medium,
                                Separation = SeparationStyle.Strong,
                                Images = league.MoreImages.Select(img => new Image()
                                {
                                    Url = img
                                }).ToList()
                            }
                        },
                        SelectAction = new OpenUrlAction()
                        {
                             Url = "https://www.fifa.com/worldcup/"
                        }
                    }
                }
            };

            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            var reply = context.MakeMessage();
            reply.Attachments.Add(attachment);

            await context.PostAsync(reply, CancellationToken.None);
        }


        private static AdaptiveCard GetFootballLivescoreCard()
        {
            return new AdaptiveCard()
            {
                Body = new List<CardElement>()
                {
                        // Footballs Search form
                        new TextBlock()
                        {
                            Text = "Welcome to the Football Carnival!",
                            Speak = "<s>Welcome to the Football Carnival!</s>",
                            Weight = TextWeight.Bolder,
                            Size = TextSize.Large
                        },
                        new TextBlock() { Text = "Please enter your team 1:" },
                        new TextInput()
                        {
                            Id = "Team1",
                            Speak = "<s>Please enter your team 1</s>",
                            Placeholder = "France, Brasil,etc",
                            Style = TextInputStyle.Text
                        },
                        new TextBlock() { Text = "Please enter your team 2:" },
                        new TextInput()
                        {
                            Id = "Team2",
                            Speak = "<s>Please enter your team 2</s>",
                            Placeholder = "France, Brasil,etc",
                            Style = TextInputStyle.Text
                        },
                        //new TextBlock() { Text = "How many nights do you want to stay?" },
                        //new NumberInput()
                        //{
                        //    Id = "Nights",
                        //    Min = 1,
                        //    Max = 60,
                        //    Speak = "<s>How many nights do you want to stay?</s>"
                        //}
                },
                Actions = new List<ActionBase>()
                {
                    new SubmitAction()
                    {
                        Title = "Search",
                        Speak = "<s>Search</s>",
                        DataJson = "{ \"Type\": \"LivescoreSearch\" }"
                    }
                }
            };
        }
    }
}