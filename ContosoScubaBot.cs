using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdaptiveCards;
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
using Razorback;
using static Microsoft.Bot.Builder.Prompts.Choices.Channel;

namespace ContosoScuba.Bot
{
    public class ContosoScubaBot : IBot
    {
        private readonly IRazorbackTemplateEngine _razorback;
        public ContosoScubaBot(IRazorbackTemplateEngine razorback)
        {
            _razorback = razorback;
        }

        public async Task OnTurn(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                //if the message is proxied between users, then do not treat it as a normal reservation message
                if (!await ChatProxied(context))
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
            else if (context.Activity.Type == ActivityTypes.ConversationUpdate && context.Activity.ChannelId != Channels.Directline)
            {
                IConversationUpdateActivity iConversationUpdated = context.Activity as IConversationUpdateActivity;
                if (iConversationUpdated != null)
                {
                    foreach (var member in iConversationUpdated.MembersAdded ?? System.Array.Empty<ChannelAccount>())
                    {
                        // if the bot is added, then show welcome message
                        if (member.Id == iConversationUpdated.Recipient.Id)
                        {
                            var reply = await context.Activity.GetReplyFromCardAsync("0-Welcome");
                            await context.SendActivity(reply);
                        }
                    }
                }
            }
            else if (context.Activity.Type == ActivityTypes.Event && context.Activity.Name == "WelcomeRequest")
            {
                var reply = await context.Activity.GetReplyFromCardAsync("0-Welcome");
                await context.SendActivity(reply);
            }
            else if (context.Activity.Type == ActivityTypes.Event && context.Activity.Name == "proxyWelcomeRequest")
            {
                var channelData = context.Activity.ChannelData as JObject;
                if (channelData != null)
                {
                    //messages with a "chatwithuserid" in channel data are from a Contoso Scuba instructor, chatting a person who has made a new reservation
                    JToken userIdToken = null;
                    if (channelData.TryGetValue("chatwithuserid", out userIdToken))
                    {
                        var userName = ReservationSubscriptionService.GetUserName(userIdToken.ToString());
                        var reply = context.Activity.CreateReply($"You are now messaging: {userName} ");
                        await context.SendActivity(reply);
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
                //messages with a "chatwithuserid" in channel data are from a Contoso Scuba instructor, chatting a person who has made a new reservation
                JToken userIdToken = null;
                if (channelData.TryGetValue("chatwithuserid", out userIdToken))
                {
                    var userId = userIdToken.ToString();
                    var conversationRef = new ConversationReference(context.Activity.Id, context.Activity.From, context.Activity.Recipient, context.Activity.Conversation, context.Activity.ChannelId, context.Activity.ServiceUrl);

                    var proxyMessage = new ProxyMessage()
                    {
                        //todo: hard coded instructor image url
                        ImageUrl = "https://raw.githubusercontent.com/matthidinger/ContosoScubaBot/master/wwwroot/Assets/scubabackground.jpeg",
                        Name = context.Activity.From.Name,
                        Text = context.Activity.Text,
                        Title = "Instructor Message"
                    };

                    var message = await GetMessageFromRazorback<ProxyMessage>(context, "ProxyMessageView", proxyMessage);

                    await ReservationSubscriptionService.ForwardToReservationUser(userId, message, context.Adapter, credentials, conversationRef);

                    return true;
                }
            }

            if (ReservationSubscriptionService.UserIsMessagingSubscriber(context.Activity.From.Id))
            {
                var proxyMessage = new ProxyMessage()
                {
                    //todo: hard coded instructor image url
                    ImageUrl = "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg",
                    Name = context.Activity.From.Name, //todo: use customer's name
                    Text = context.Activity.Text,
                    Title = "Customer Message"
                };

                var message = await GetMessageFromRazorback<ProxyMessage>(context, "ProxyMessageView", proxyMessage);

                return await ReservationSubscriptionService.ForwardedToSubscriber(context.Activity.From.Id, message, context.Adapter, credentials);
            }
            return false;
        }

        private async Task<IMessageActivity> GetNextScubaMessage(ITurnContext context, Activity activity)
        {
            var resultInfo = await new ScubaCardService().GetNextCardText(context, activity);
            if (!string.IsNullOrEmpty(resultInfo.ErrorMessage))
            {
                var reply = activity.CreateReply(resultInfo.ErrorMessage);
                //cortana's turned based nature requires a 'back' button when validation fails
                if (activity.ChannelId == Channel.Channels.Cortana)
                {
                    var backCard = new AdaptiveCard("1.0");
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

            return activity.GetReplyFromText(resultInfo.CardText);
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
                nextMessage = await GetSampleMessage<WildlifeData>(context, "WildlifeView");
            }
            else if (text.Contains("receipt"))
            {
                nextMessage = await activity.GetReplyFromCardAsync("Receipt");
            }
            else if (text.Contains("danger"))
            {
                nextMessage = await activity.GetReplyFromCardAsync("Danger");
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
                ReservationSubscriptionService.RemoveUserConnectionToSubscriber(activity.From.Id);
                nextMessage = await activity.GetReplyFromCardAsync("0-Welcome");
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
                    var userScubaData = SampleData.GetData<UserScubaData>();
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

        async Task<IMessageActivity> GetSampleMessage<T>(ITurnContext context, string viewName)
            where T : class
        {
            var data = SampleData.GetData<T>();
            return await GetMessageFromRazorback<T>(context, viewName, data);
        }

        async Task<IMessageActivity> GetMessageFromRazorback<T>(ITurnContext context, string viewName, T data)
            where T : class
        {
            var card = await this._razorback.BindXmlToObject<T, AdaptiveCard>(viewName, data);

            var message = context.Activity.CreateReply();
            message.Attachments.Add(new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            });

            return message;
        }
    }
}
