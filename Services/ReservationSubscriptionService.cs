using ContosoScuba.Bot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using static Microsoft.Bot.Builder.Prompts.Choices.Channel;
using ContosoScuba.Bot.CardProviders;
using System.Text;
using Newtonsoft.Json;

namespace ContosoScuba.Bot.Services
{
    public static class ReservationSubscriptionService
    {
        //all who have subscribed to receive notifications when a customer reserves a scuba getaway(key: subscriber userId, conversationRefernce: subscriber)
        private static ConcurrentDictionary<string, ConversationReference> _reservationSubscribers = new ConcurrentDictionary<string, ConversationReference>();
        //all who have made a scuba reservation (key: customer userId, conversationRefernce: customer webchat)
        private static ConcurrentDictionary<string, ConversationReference> _recentReservations = new ConcurrentDictionary<string, ConversationReference>();
        //subscribers who have begun chatting with a user via proxy of webchat messages (key: customer userId, conversationRefernce: subscriber webchat)
        private static ConcurrentDictionary<string, ConversationReference> _subscriberToUser = new ConcurrentDictionary<string, ConversationReference>();
       
        #region Subscribers

        public static void AddOrUpdateSubscriber(string userId, ConversationReference conversationReference)
        {
            _reservationSubscribers.AddOrUpdate(userId, conversationReference, (key, oldValue) => conversationReference);
        }

        public static void RemoveSubscriber(string userId)
        {
            ConversationReference reference = null;
            _reservationSubscribers.TryRemove(userId, out reference);
        }
        
        public static async Task SendActionableMessage(UserScubaData userScubaData)
        {
            var client = new HttpClient();
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userScubaData)));
            await client.PostAsync("https://adaptivetestfunctions.azurewebsites.net/api/SendScubaEmail?code=4tRDT5xalBkFidaesDGNSg1xVRcU2HPh7Ar7Zsc8vpAXE8DdG9mzHg==", content);


            //var myData = new MyDataObject();
            //myData.EventName = "Publish adaptive card schema";
            //myData.ProfileImage = "http://.....";

            //var card = AdaptiveCard.FromJson("thetemplate.json", myData);

            //return card.ToJson();


            //var card = new AdaptiveCard();
            //card.Body.Add(new TextBlock() { Text = "sdsds" });
            //return card.ToJson();
        }
        public static IEnumerable<string> GetSubscribers()
        {
            foreach (var subscriber in _reservationSubscribers.Values)
            {
                yield return subscriber.User.Name;
            }
        }

        public static async Task NotifySubscribers(UserScubaData userScubaData, BotAdapter adapter, MicrosoftAppCredentials workingCredentials, ConversationReference reserverReference = null)
        {
            if (reserverReference != null)
            {
                _recentReservations.AddOrUpdate(reserverReference.User.Id, reserverReference, (key, oldValue) => reserverReference);
                //todo: this should not be a hard coded url
                userScubaData.ChatWithUserUrl = "https://contososcubademo.azurewebsites.net?chatWithId=" + reserverReference.User.Id;
                //chatWithUserIdUrl = "Use this URL to chat with them: http://localhost:3979?chatWithId=" + reserverReference.User.Id;
            }
            string message = $"New reservation for {userScubaData.PersonalInfo.Name}. Use this URL to chat with them: {userScubaData.ChatWithUserUrl}";

            var replaceInfo = new Dictionary<string, string>();
            replaceInfo.Add("{{destination}}", userScubaData.School);
            DateTime date = Convert.ToDateTime(userScubaData.Date);
            replaceInfo.Add("{{longdate}}", date.ToString("dddd, MMMM dd"));
            replaceInfo.Add("{{number_of_people}}", userScubaData.NumberOfPeople);
            replaceInfo.Add("{{phone}}", userScubaData.PersonalInfo.Phone);
            replaceInfo.Add("{{email}}", userScubaData.PersonalInfo.Email);
            replaceInfo.Add("{{name}}", userScubaData.PersonalInfo.Name);
            if(!string.IsNullOrEmpty(userScubaData.ChatWithUserUrl))
                replaceInfo.Add("{{url}}", userScubaData.ChatWithUserUrl);

            var subscriberCardText = await CardProvider.GetCardText("SubscriberNotification", replaceInfo);
            var conversationCallback = GetConversationCallback(message, workingCredentials, subscriberCardText);

            await SendActionableMessage(userScubaData);

            foreach (var subscriber in _reservationSubscribers.Values)
            {
                await adapter.ContinueConversation(subscriber.Bot.Id, subscriber, conversationCallback);
            }
        }

        #endregion Subscribers

        #region Users

        public static async Task ForwardToReservationUser(string userId, string message, BotAdapter adapter, MicrosoftAppCredentials workingCredentials, ConversationReference contosoReference)
        {
            ConversationReference foundReference = null;
            if(_recentReservations.TryGetValue(userId, out foundReference))
            {
                _subscriberToUser.AddOrUpdate(userId, contosoReference, (key, oldValue) => contosoReference);
                Func<ITurnContext, Task> conversationCallback = GetConversationCallback($"Instructor Message: {message}", workingCredentials);
                await adapter.ContinueConversation(foundReference.Bot.Id, foundReference, conversationCallback);
            }
        }

        public static async Task<bool> ForwardedToSubscriber(string userId, string message, BotAdapter adapter, MicrosoftAppCredentials workingCredentials)
        {
            ConversationReference foundReference = null;
            if (_subscriberToUser.TryGetValue(userId, out foundReference))
            {
                Func<ITurnContext, Task> conversationCallback = GetConversationCallback($"Customer Message: {message}", workingCredentials);
                await adapter.ContinueConversation(foundReference.Bot.Id, foundReference, conversationCallback);
                return true;
            }

            return false;
        }

        #endregion Users

        private static Func<ITurnContext, Task> GetConversationCallback(string text, MicrosoftAppCredentials workingCredentials, string fullMessageText = null)
        {
            Func<ITurnContext, Task> conversationCallback = async (context) =>
            {
                //TODO: bug in connector credentials (password is blank, and appid is bot id depending on channel that sent the message)
                var contextCredentials = ((MicrosoftAppCredentials)context.Services.Get<Microsoft.Bot.Connector.IConnectorClient>("Microsoft.Bot.Connector.IConnectorClient").Credentials);
                contextCredentials.MicrosoftAppId = workingCredentials.MicrosoftAppId;
                contextCredentials.MicrosoftAppPassword = workingCredentials.MicrosoftAppPassword;

                Activity reply = null;
                if(string.IsNullOrEmpty(fullMessageText))
                {
                    reply = context.Activity.CreateReply(text);
                }
                else
                {
                    reply = ContosoScubaBot.GetCardReply(context.Activity, fullMessageText);
                    reply.Text = text;
                }
                await context.SendActivity(reply);
            };

            return conversationCallback;
        }
    }
}
