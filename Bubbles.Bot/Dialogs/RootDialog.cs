using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Web.Hosting;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Bubbles.Bot.Utils;

namespace Bubbles.Bot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            var nextMessage = await GetNextMessage(context, activity);
            await context.PostAsync(nextMessage);
            context.Wait(MessageReceivedAsync);
        }

        private async Task<IMessageActivity> GetNextMessage(IDialogContext context, Activity activity)
        {
            var cardText = await new CardFinder().GetNextCardText(activity);
            return GetReply(activity, cardText);
        }

        private Activity GetReply(Activity activity, string cardText)
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