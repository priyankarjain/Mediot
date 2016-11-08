using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using HtmlAgilityPack;

namespace webrequest
{   [Serializable]
    public class MainDialog : IDialog<object>
    {
        public string url;
        public string s_url;
        public int trigger;
        public int s_num;

        public MainDialog()
        {
            url = "";
            s_url = "";
            s_num = 0;
            trigger = 0;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {

            var message = await result;
            string data = message.Text;
            char[] delimiter = { ' ' };
            string[] command = data.Split(delimiter);
            StringBuilder sb = new StringBuilder();

            //if (command.Length > 1)
            //{


            if (trigger == 0)
            {
                for (int i = 1; i < command.Length; i++)
                {
                    if (command.Length > 1)
                        sb.Append(command[i] + " ");
                }

                switch (command[0].ToLower())
                {
                    case "search":
                        await createSearchResponse(context, sb);
                        break;
                    case "showid":
                        url = "http://api.tvmaze.com/shows/" + HttpUtility.UrlEncode(sb.ToString().Trim());
                        await createShowResponse(context, url);
                        await sendOptions(context);
                        break;
                    case "cast":
                        if (url != "")
                        {
                            string casturl = url + "/cast";
                            await createCastResponse(context, casturl);
                            await sendOptions(context);
                        }
                        break;
                    case "epbn":
                        if (url != "") {
                            trigger = 1;
                            await context.PostAsync("Enter Season number and Episode number separated by space");
                        }
                        break;
                    case "clear":
                        url = "";
                        break;

                    case "season":
                        if (url != "")
                        {
                            string seasonurl = url + "/seasons";
                            s_url = seasonurl;
                            await createSeasonResponse(context, seasonurl);
                        }
                        break;
                    case "selectseason":
                        if (s_url != "")
                        {
                            s_num = int.Parse(command[1]);
                            trigger = 2;
                            await context.PostAsync("Enter the episode number for this season");
                            
                            
                        }
                        break;
                   default:
                        await context.PostAsync("I dont understand this");
                        break;
                }
            }else if(trigger==1)
            {
                trigger = 0;
                int season = int.Parse(command[0]);
                int ep = int.Parse(command[1]);

                string epurl = url+$"/episodebynumber?season={season}&number={ep}";
                HttpWebRequest request = WebRequest.Create(epurl) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string s = reader.ReadToEnd();
                Declarations.Episodes.Rootobject episode = JsonConvert.DeserializeObject<Declarations.Episodes.Rootobject>(s);

                await context.PostAsync(createEpisodeMessage(context,episode));
                await sendOptions(context);
            }else if (trigger == 2)
            {
                trigger = 0;
                int epno = int.Parse(command[0]);
                
                string epurl = url + $"/episodebynumber?season={s_num}&number={epno}";
                HttpWebRequest request = WebRequest.Create(epurl) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string s = reader.ReadToEnd();
                Declarations.Episodes.Rootobject episode = JsonConvert.DeserializeObject<Declarations.Episodes.Rootobject>(s);

                await context.PostAsync(createEpisodeMessage(context, episode));
                await sendOptions(context);
            }
            
            context.Wait(MessageReceivedAsync);
        }
       
        private async Task createSeasonResponse(IDialogContext context, string seasonurl)
        {
            IMessageActivity message = context.MakeMessage();
            HttpWebRequest request = WebRequest.Create(seasonurl) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            List<Declarations.Seasons.Rootobject> seasons = JsonConvert.DeserializeObject<List<Declarations.Seasons.Rootobject>>(s);
            response.Close();
            reader.Close();
            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            message.Attachments = new List<Attachment>();
            foreach(Declarations.Seasons.Rootobject season in seasons)
            {
                if (season.image == null)
                {
                    season.image = new Declarations.Seasons.Image();
                    season.image.medium = "http://www.herniamovers.com/assets/boxes_packages/large/image_not_available.gif";
                    season.image.original = "http://www.herniamovers.com/assets/boxes_packages/large/image_not_available.gif";
                }
                List<CardImage> image = new List<CardImage>();
                image.Add(new CardImage()
                {
                    Url = season.image.medium
                });
                CardAction action = new CardAction()
                {
                    Type="imBack",
                    Value=$"selectseason {season.number}"
                };
                if (season.summary == null)
                {
                    season.summary = "";
                }
                string order;
                if (season.episodeOrder == 0)
                {
                    
                    order = "Not Available";
                }else
                {
                    order = season.episodeOrder.ToString();
                }
                ThumbnailCard card = new ThumbnailCard()
                {
                    Title = "Season " + season.number,
                    Subtitle = $"Start:{season.premiereDate} \r\n\r\n End:{season.endDate}",
                    Images = image,
                    Text = $"Number of episodes: {order}",
                    Tap=action                 
                };

                message.Attachments.Add(card.ToAttachment());
            }

            await context.PostAsync(message);
        }

        public async Task createCastResponse(IDialogContext context, string casturl)
        {
            IMessageActivity message = context.MakeMessage();
            HttpWebRequest request = WebRequest.Create(casturl) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string s = reader.ReadToEnd();
            List<Declarations.People.RootObject> cast = JsonConvert.DeserializeObject<List<Declarations.People.RootObject>>(s);
            response.Close();
            reader.Close();

            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            message.Attachments = new List<Attachment>();

            foreach (Declarations.People.RootObject Person in cast)
            {

                if (Person.person.image == null)
                {
                    Person.person.image = new Declarations.People.Image();
                    Person.person.image.medium = "http://www.herniamovers.com/assets/boxes_packages/large/image_not_available.gif";
                    Person.person.image.original = "http://www.herniamovers.com/assets/boxes_packages/large/image_not_available.gif";
                }

                List<CardImage> image = new List<CardImage>();
                image.Add(new CardImage(Person.person.image.medium));

                List<CardAction> button = new List<CardAction>();

                button.Add(new CardAction()
                {
                    Title = "view on google",
                    Type = "openUrl",
                    Value = "http://www.google.com/search?q=" + HttpUtility.UrlEncode(Person.person.name)
                 });
        
            HeroCard card = new HeroCard()
            {
                Title = Person.person.name,
                Subtitle = Person.character.name,
                Images = image,
                Buttons = button
            };
            message.Attachments.Add(card.ToAttachment());
        
          }
            await context.PostAsync(message);

        }

        private async Task sendOptions(IDialogContext context)
        {
            IMessageActivity options = context.MakeMessage();
            List<CardAction> buttons = new List<CardAction>();
            buttons.Add(new CardAction()
            {
                Title = "Seasons",
                Type = "imBack",
                Value = "season"
            });
            buttons.Add(new CardAction()
            {
                Title = "Episode by number",
                Type = "imBack",
                Value = "epbn"
            });
            buttons.Add(new CardAction()
            {
                Title = "Cast",
                Type = "imBack",
                Value = "cast"
            });
            /*buttons.Add(new CardAction()
            {
                Title = "Crew",
                Type = "imBack",
                Value = "crew"
            });*/
            HeroCard card = new HeroCard()
            {
                Buttons = buttons
            };
            options.Attachments = new List<Attachment>();
            options.Attachments.Add(card.ToAttachment());
            await context.PostAsync(options);
        }

        private async Task createShowResponse(IDialogContext context, String url)
        {
            
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            StreamReader reader = new StreamReader(response.GetResponseStream());
            
            
            string ans = reader.ReadToEnd();
            reader.Close();
            response.Close();

            Declarations.Shows.Show show = JsonConvert.DeserializeObject<Declarations.Shows.Show>(ans);
           
            await context.PostAsync(createShowMessage(context,show));
        }

        public IMessageActivity createShowMessage(IDialogContext context,Declarations.Shows.Show show)
        {
            IMessageActivity message = context.MakeMessage();
            string sub;
            string time = "";
            if (show.network == null)
            {
                if (show.webChannel != null)
                {
                    sub = show.webChannel.name;
                    time = "";
                }
                else
                {
                    sub = "Channel not available";
                }
            }
            else
            {
                sub = show.network.name;
                time = show.schedule.time;
            }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(show.summary);
            string summary="";
            if (show.summary == "")
            {
                summary = "Not Available";

            }
            else
            {
                HtmlNodeCollection l = doc.DocumentNode.SelectNodes("//p");
                foreach (HtmlNode nodes in l)
                {
                    summary += nodes.InnerText;
                }
            }

            StringBuilder gen = new StringBuilder();
            foreach (string g in show.genres)
            {
                gen.Append(g + ", ");
            }
            string s = "";
            foreach (string d in show.schedule.days)
            {
                s += d;
            }
            StringBuilder res = new StringBuilder();


            res.Append("# " + show.name).AppendLine().AppendLine();
            res.Append(" **Channel:** " + sub).AppendLine().AppendLine();
            res.Append("**Status:** " + show.status).AppendLine().AppendLine();
            res.Append("**Genre:** " + gen.ToString()).AppendLine().AppendLine();
            res.Append("**Premiere:** " + show.premiered).AppendLine().AppendLine();
            res.Append("**Schedule:** " + time + $" {s}").AppendLine().AppendLine();
            res.Append($"**Summary:** {summary}");

            message.Text = res.ToString();
            return message;
        } 

        private async Task createSearchResponse(IDialogContext context,StringBuilder sb)
        {
            IMessageActivity message = context.MakeMessage();
            
            HttpWebRequest request = WebRequest.Create("http://api.tvmaze.com/search/shows/?q=" + HttpUtility.UrlEncode(sb.ToString().Trim())) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string ans = reader.ReadToEnd();
            reader.Close();
            response.Close();

            List<Declarations.Shows.RootObject> shows =JsonConvert.DeserializeObject<List<Declarations.Shows.RootObject>>(ans);
            
            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            message.Attachments = new List<Attachment>();

            foreach (Declarations.Shows.RootObject soap in shows)
            {
                string sub;

                if (soap.show.image == null)
                {
                    soap.show.image = new Declarations.Shows.Image();
                    soap.show.image.medium = "http://www.herniamovers.com/assets/boxes_packages/large/image_not_available.gif";
                    soap.show.image.original="http://www.herniamovers.com/assets/boxes_packages/large/image_not_available.gif";
                }

                List<CardImage> image = new List<CardImage>();
                image.Add(new CardImage(soap.show.image.medium));

                List<CardAction> button = new List<CardAction>();

                button.Add(new CardAction(){
                    Title = "Tap to Select",
                    Type = "imBack",
                    Value = "showid "+soap.show.id
                });
                if (soap.show.network == null)
                {
                    if (soap.show.webChannel != null)
                    {
                        sub = soap.show.webChannel.name;
                    }
                    else
                    {
                        sub = "Channel not available";
                    }
                }else
                {
                    sub = soap.show.network.name;
                }
                HeroCard card = new HeroCard()
                {
                    Title=soap.show.name,
                    Subtitle=sub,
                    Images=image,
                    Buttons=button
                };
                message.Attachments.Add(card.ToAttachment());
            }
            await context.PostAsync(message);
        }

        public IMessageActivity createEpisodeMessage(IDialogContext context,Declarations.Episodes.Rootobject episode)
        {
            IMessageActivity message = context.MakeMessage();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(episode.summary);
            if (episode.image == null)
            {
                episode.image = new Declarations.Episodes.Image();
                episode.image.medium = "http://www.herniamovers.com/assets/boxes_packages/large/image_not_available.gif";
                episode.image.original = "http://www.herniamovers.com/assets/boxes_packages/large/image_not_available.gif";
            }


            /*message.Attachments.Add(new Attachment() {
                ContentType="image/jpg",
                ContentUrl=episode.image.medium,
            });*/
            string summary="";
            if (episode.summary == "" || episode.summary==null)
            {
                summary = "Not Available";
                
            }
            else
            {             
               HtmlNodeCollection l= doc.DocumentNode.SelectNodes("//p");
               foreach(HtmlNode nodes in l)
                {
                    summary+=nodes.InnerText;
                }
            }
            StringBuilder res = new StringBuilder();


            res.Append("# " + episode.name).AppendLine().AppendLine();
            res.Append($"![]({episode.image.medium})");
            res.AppendLine().AppendLine().Append(" **airdate:** " + episode.airdate).AppendLine().AppendLine();
            res.Append("**Time:** " + episode.airtime).AppendLine().AppendLine();
            res.Append("**Episode Runtime:** " + episode.runtime.ToString()+" mins").AppendLine().AppendLine();
            res.Append($"**Summary:** {summary}");

            message.Text = res.ToString();
            return message;
        }
    }
}