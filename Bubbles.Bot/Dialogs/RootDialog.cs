using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Web.Hosting;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Bubbles.Bot.Services;
using System.Threading;
using static Bubbles.Bot.Services.NextScubaCardService;

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
            string text = string.IsNullOrEmpty(activity.Text) ? string.Empty : activity.Text.ToLower();
            if (!string.IsNullOrEmpty(text) && text.Contains("weather"))
            {
                // await Conversation.SendAsync(activity, () => new Dialogs.WeatherDialog());
                var weatherDialog = new Dialogs.WeatherDialog();
                await context.Forward(weatherDialog, AfterWeatherDialog, activity, CancellationToken.None);
            }
            else if (!string.IsNullOrEmpty(text) && text.Contains("wildlife"))
            {
                var nextMessage = await GetCard(context, activity, "ImageSet-JoeB");
                await context.PostAsync(nextMessage);
                context.Wait(MessageReceivedAsync);
            }
            else if (!string.IsNullOrEmpty(text) && text.Contains("danger"))
            {
                var nextMessage = await GetCard(context, activity, "Danger-BF");
                await context.PostAsync(nextMessage);
                context.Wait(MessageReceivedAsync);
            }
            else if (!string.IsNullOrEmpty(text) &&  text.Contains("meal"))
            {
                var nextMessage = await GetCard(context, activity, "ScubaReservation");
                await context.PostAsync(nextMessage);
                context.Wait(MessageReceivedAsync);
            }
            else
            { 
                if(text=="hi"||text=="hello")
                {
                    context.ConversationData.Clear();          
                }
                var nextMessage = await GetNextMessage(context, activity);
                await context.PostAsync(nextMessage);
                context.Wait(MessageReceivedAsync);
            }            
        }

        private async Task AfterWeatherDialog(IDialogContext context, IAwaitable<object> result)
        {
            //var messageHandled = await result;
            //if (!messageHandled)
            //{
            //    await context.PostAsync("Sorry, I wasn't able to find the weather for you.");
            //}

            context.Wait(MessageReceivedAsync);

        }

        private async Task<IMessageActivity> GetCard(IDialogContext context, Activity activity, string cardName)
        {
            var transitionInfo = new NextScubaCardService.CardTransitionInfo();
            transitionInfo.NextCardName = cardName;
            var cardText = await new NextScubaCardService().GetCardText(transitionInfo);
            return GetReply(activity, cardText);
        }

        private async Task<IMessageActivity> GetNextMessage(IDialogContext context, Activity activity)
        {
            var cardText = await new NextScubaCardService().GetNextCardText(context, activity);
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