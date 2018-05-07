using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using ContosoScuba.Bot.Services;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Microsoft.Bot.Builder.Prompts.Choices.Channel;

namespace ContosoScuba.Bot
{
    public class ContosoScubaBot : IBot
    {
        public async Task OnTurn(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                //if the message is proxied between users, then do not treat it as a normal reservation message
                if (! await ChatProxied(context))
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
            }
            else if (context.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                IConversationUpdateActivity iConversationUpdated = context.Activity as IConversationUpdateActivity;
                if (iConversationUpdated != null)
                {
                    foreach (var member in iConversationUpdated.MembersAdded ?? System.Array.Empty<ChannelAccount>())
                    {
                        // if the bot is added, then show welcome message
                        if (member.Id == iConversationUpdated.Recipient.Id)
                        {
                            var cardText = await ScubaCardService.GetCardText("0-Welcome");
                            var reply = GetCardReply(context.Activity, cardText);
                            await context.SendActivity(reply);
                        }
                    }
                }
            }
        }

        private async Task<bool> ChatProxied(ITurnContext context)
        {
            var credentials = ((MicrosoftAppCredentials)context.Services.Get<Microsoft.Bot.Connector.IConnectorClient>("Microsoft.Bot.Connector.IConnectorClient").Credentials);

            var channelData = context.Activity.ChannelData as JObject;
            if (channelData != null)
            {
                //messages with a "chatwithuserid" in channel data are from a Contoso Scuba employee, chatting a person who has made a new reservation
                JToken userIdToken = null;
                if (channelData.TryGetValue("chatwithuserid", out userIdToken))
                {
                    var userId = userIdToken.ToString();                    
                    var conversationRef = new ConversationReference(context.Activity.Id, context.Activity.From, context.Activity.Recipient, context.Activity.Conversation, context.Activity.ChannelId, context.Activity.ServiceUrl);
                    await ReservationSubscriptionService.ForwardToReservationUser(userId, context.Activity.Text, context.Adapter, credentials, conversationRef);

                    return true;
                }
            }

            return await ReservationSubscriptionService.ForwardedToSubscriber(context.Activity.From.Id, context.Activity.Text, context.Adapter, credentials);
        }

        private async Task<IMessageActivity> GetNextScubaMessage(ITurnContext context, Activity activity)
        {
            var resultInfo = await new ScubaCardService().GetNextCardText(context, activity);
            if (!string.IsNullOrEmpty(resultInfo.ErrorMessage))
            {
                var reply = activity.CreateReply(resultInfo.ErrorMessage);
                if (activity.ChannelId == Channel.Channels.Cortana)
                {
                    var backCard = new AdaptiveCards.AdaptiveCard();
                    backCard.Actions.Add(new AdaptiveCards.AdaptiveSubmitAction()
                    {
                        Data = "Back",
                        Title = "Back"
                    });
                    reply.Attachments.Add(new Attachment()
                    {
                        Content = backCard,
                        ContentType = AdaptiveCards.AdaptiveCard.ContentType
                    });
                }
                return reply;
            }

            if (resultInfo.NotifySubscribers)
            {
                var adapter = context.Adapter;
                var userScubaData = context.GetConversationState<UserScubaData>();
                var credentials = ((MicrosoftAppCredentials)context.Services.Get<Microsoft.Bot.Connector.IConnectorClient>("Microsoft.Bot.Connector.IConnectorClient").Credentials);

                var conversationRef = new ConversationReference(activity.Id, activity.From, activity.Recipient, activity.Conversation, activity.ChannelId, activity.ServiceUrl);

                Task.Factory.StartNew(async () => await ReservationSubscriptionService.NotifySubscribers(userScubaData, adapter, credentials, conversationRef));
            }

            return GetCardReply(activity, resultInfo.CardText);
        }

        private async Task<IMessageActivity> GetMessageFromText(ITurnContext context, Activity activity, string text)
        {
            IMessageActivity nextMessage = null;


            if (text.StartsWith("/"))
            {
                nextMessage = GetSlashCommandMessage(context, text, activity);
            }
            else if (text.Contains("wildlife"))
            {
                nextMessage = await GetCard(activity, "Wildlife");
            }
            else if (text.Contains("receipt"))
            {
                nextMessage = await GetCard(activity, "Receipt");
            }
            else if (text.Contains("danger"))
            {
                nextMessage = await GetCard(activity, "Danger");
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

        private IMessageActivity GetSlashCommandMessage(ITurnContext context, string text, Activity activity)
        {
            text = text.Replace("/", "");
            if (activity.ChannelId == Channels.Msteams)
            {
                if (text.Contains("simulate"))
                {
                    var adapter = context.Adapter;
                    var userScubaData = new UserScubaData()
                    {
                        School = "Fabrikam",
                        Destination = "Adventure Works",
                        NumberOfPeople = "6",
                        Date = DateTime.Now.AddDays(4).ToString(),
                        MealOptions = new MealOptions()
                        {
                            Alergy = "none",
                            ProteinPreference = "Beef",
                            Vegan = false
                        },
                        PersonalInfo = new PersonalInfo()
                        {
                            Email = "customeremail@microsoft.com",
                            Name = "Customer Name",
                            Phone = "888.888.7000"
                        }
                    };
                    var credentials = ((MicrosoftAppCredentials)context.Services.Get<Microsoft.Bot.Connector.IConnectorClient>("Microsoft.Bot.Connector.IConnectorClient").Credentials);
                    Task.Factory.StartNew(async () => await ReservationSubscriptionService.NotifySubscribers(userScubaData, adapter, credentials));
                    return activity.CreateReply("Simulating reservation and notifying subscribers");
                }
                else if (text.Contains("subscribers"))
                {
                    var subscribers = ReservationSubscriptionService.GetSubscribers();
                    return activity.CreateReply($"Subscribers: {string.Join(", ", subscribers)}");
                }
                else if (text.Contains("unsubscribe"))
                {
                    ReservationSubscriptionService.RemoveSubscriber(activity.From.Id);
                    return activity.CreateReply("You are now unsubscribed from all Contoso Dive Finder reservations.");
                }
                else if (text.Contains("subscribe"))
                {
                    var conversationRef = new ConversationReference(activity.Id, activity.From, activity.Recipient, activity.Conversation, activity.ChannelId, activity.ServiceUrl);
                    ReservationSubscriptionService.AddOrUpdateSubscriber(activity.From.Id, conversationRef);
                    return activity.CreateReply("You are now subscribed to all Contoso Dive Finder reservations.");
                }
            }
            return null;
        }

        public static async Task<IMessageActivity> GetCard(Activity activity, string cardName)
        {
            var cardText = await ScubaCardService.GetCardText(cardName);
            return GetCardReply(activity, cardText);
        }

        public static Activity GetCardReply(Activity activity, string activityText)
        {
            var reply = JsonConvert.DeserializeObject<Activity>(activityText);
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
