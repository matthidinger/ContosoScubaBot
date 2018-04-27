using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using ContosoScuba.Bot.Services;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace ContosoScuba.Bot
{
    public class ContosoScubaBot : IBot
    {
        public async Task OnTurn(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                //scuba bot allows entering text, or interacting with the card
                string text = string.IsNullOrEmpty(context.Activity.Text) ? string.Empty : context.Activity.Text.ToLower();

                IMessageActivity nextMessage = null;

                if (!string.IsNullOrEmpty(text))
                {
                    nextMessage = await GetMessageFromText(context, context.Activity, text);
                }

                if (nextMessage == null)
                    nextMessage = await GetNextScubaMessage(context, context.Activity);

                await context.SendActivity(nextMessage);
            }
            else if (context.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                // On "conversationUpdate"-type activities this bot will send a greeting message to users joining the conversation.
                var newUserName = context.Activity.MembersAdded.FirstOrDefault()?.Name;
                if (!string.IsNullOrWhiteSpace(newUserName) && newUserName != "Bot")
                {
                    var cardText = await ScubaCardService.GetCardText("0-Welcome");
                    var reply = GetCardReply(context.Activity, cardText);
                    await context.SendActivity(reply);
                }
            }
        }
        private async Task<IMessageActivity> GetNextScubaMessage(ITurnContext context, Activity activity)
        {
            var resultInfo = await new ScubaCardService().GetNextCardText(context, activity);
            if (!string.IsNullOrEmpty(resultInfo.ErrorMessage))
                return activity.CreateReply(resultInfo.ErrorMessage);

            return GetCardReply(activity, resultInfo.CardText);
        }

        private async Task<IMessageActivity> GetMessageFromText(ITurnContext context, Activity activity, string text)
        {
            IMessageActivity nextMessage = null;


            if (text.Contains("wildlife"))
            {
                return nextMessage = await GetCard(activity, "Wildlife");
            }
            else if (text.Contains("receipt"))
            {
                return nextMessage = await GetCard(activity, "Receipt");
            }
            else if (text.Contains("danger"))
            {
                return nextMessage = await GetCard(activity, "Danger");
            }
            else if (text == "hi"
                     || text == "hello"
                     || text == "reset"
                     || text == "start over"
                     || text == "restart")
            {
                //clear conversation data, since the user has decided to restart
                var userScubaState = context.GetConversationState<UserScubaData>();
                userScubaState.Clear();
                nextMessage = await GetCard(activity, "0-Welcome");
            }

            return nextMessage;
        }

        private async Task<IMessageActivity> GetCard(Activity activity, string cardName)
        {
            var cardText = await ScubaCardService.GetCardText(cardName);
            return GetCardReply(activity, cardText);
        }

        public static Activity GetCardReply(Activity activity, string cardText)
        {
            var reply = JsonConvert.DeserializeObject<Activity>(cardText);
            if (reply.Attachments == null)
                reply.Attachments = new List<Attachment>();

            var tempReply = activity.CreateReply("");
            reply.ChannelId = tempReply.ChannelId;
            reply.Timestamp = tempReply.Timestamp;
            reply.From = tempReply.From;
            reply.Conversation = tempReply.Conversation;
            reply.Recipient = tempReply.Recipient;
            reply.Id = tempReply.Id;
            reply.ReplyToId = tempReply.ReplyToId;
            if (reply.Type == null)
                reply.Type = ActivityTypes.Message;

            return reply;
        }
    }
}
