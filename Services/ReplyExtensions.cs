using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContosoScuba.Bot.Services
{
    public static class ReplyExtensions
    {
        public static async Task<IMessageActivity> GetReplyFromCardAsync(this Activity originalActivity, string cardName)
        {
            var cardText = await ScubaCardService.GetCardText(cardName);
            return originalActivity.GetReplyFromText(cardText);
        }

        public static Activity GetReplyFromText(this Activity originalActivity, string fullReplyText)
        {
            var reply = JsonConvert.DeserializeObject<Activity>(fullReplyText);
            if (reply.Attachments == null)
                reply.Attachments = new List<Attachment>();

            originalActivity.SetReplyFields(reply);

            return reply;
        }

        public static void SetReplyFields(this Activity originalActivity, IMessageActivity reply)
        {
            var tempReply = originalActivity.CreateReply("");

            reply.ChannelId = tempReply.ChannelId;
            reply.Timestamp = tempReply.Timestamp;
            reply.From = tempReply.From;
            reply.Conversation = tempReply.Conversation;
            reply.Recipient = tempReply.Recipient;
            reply.Id = tempReply.Id;
            reply.ReplyToId = tempReply.ReplyToId;

            if (reply.Type == null)
                reply.Type = ActivityTypes.Message;
        }
    }
}