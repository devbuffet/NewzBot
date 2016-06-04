using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using HtmlAgilityPack;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;

namespace NewzBot
{
    [Serializable]
    public class NewzDialog : IDialog<object>
    {
        public string url { get; set; }

        public NewzDialog(string url)
        {
            this.url = url;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var message = await returnDownload(this.url); // get download info
            await context.PostAsync(returnNewsItems(message));
            context.Wait(MessageReceivedAsync);
        }

        /// <summary>
        /// returns download
        /// </summary>
        /// <returns></returns>
        private async Task<string> returnDownload(string url)
        {
            string download = string.Empty;

            using (WebClient client = new WebClient())
            {
                byte[] data = await client.DownloadDataTaskAsync(this.url);
                download = Encoding.UTF8.GetString(data);
            }

            // returns download info
            return await Task.FromResult<string>(download);
        }

        /// <summary>
        /// returns new items
        /// </summary>
        /// <param name="download"></param>
        /// <returns></returns>
        private string returnNewsItems(string download)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(download);

            StringBuilder resp = new StringBuilder();

            var img_nodes = doc.DocumentNode.SelectNodes("//img[contains(@class,'esc-thumbnail-image')]");

            foreach (var div in doc.DocumentNode.SelectNodes("//div[contains(@class,'esc-lead-article-title-wrapper')]"))
            {
                HtmlDocument docX = new HtmlDocument();
                docX.LoadHtml(div.InnerHtml);

                HtmlNodeCollection nodes = docX.DocumentNode.SelectNodes("//a[@href]");

                //resp.Append($"{Environment.NewLine}{Environment.NewLine}");
                //resp.Append("[" + div.InnerText + "](" + nodes[0].Attributes["href"].Value + ")");
                //resp.Append($"{Environment.NewLine}{Environment.NewLine}");

                //resp.Append(nodes[0].Attributes["href"].Value);

                resp.Append(div.InnerText);

                //resp.Append("http://www.google.com");

            }

            return resp.ToString();
        }
    }

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                // return our reply to the user
                return await Conversation.SendAsync(message, () => new NewzDialog("http://news.google.com/news/section?cf=all&pz=1&ned=us&topic=tc"));
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}