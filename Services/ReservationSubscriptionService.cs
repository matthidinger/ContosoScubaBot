using ContosoScuba.Bot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;
using Microsoft.Bot.Connector.Authentication;
using System.Net.Http;
using System.Collections.Generic;
using ContosoScuba.Bot.CardProviders;
using System.Text;
using Newtonsoft.Json;

namespace ContosoScuba.Bot.Services
{
    //This static class is a mockup message routing service when ms teams members can subscribe to 
    //be notified when a scuba booking reservation is made by a customer.  Subscribers receive a message
    //that contains a button enabling them to chat with the customer from the bot's webchat.  From that point
    //messages are proxied back and forth between the subscriber and the customer.
    public static class ReservationSubscriptionService
    {
        //all who have subscribed to receive notifications when a customer reserves a scuba getaway(key: subscriber userId, conversationRefernce: subscriber)
        private static ConcurrentDictionary<string, ConversationReference> _reservationSubscribers = new ConcurrentDictionary<string, ConversationReference>();

        //all who have made a scuba reservation (key: customer userId, conversationRefernce: customer webchat
        private static ConcurrentDictionary<string, Tuple<ConversationReference,UserScubaData>> _recentReservations = new ConcurrentDictionary<string, Tuple<ConversationReference, UserScubaData>>();
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

            // TODO: Send actionable message and push notification
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
                var scubaReservation = new Tuple<ConversationReference, UserScubaData>(reserverReference, userScubaData);
                _recentReservations.AddOrUpdate(reserverReference.User.Id, scubaReservation, (key, oldValue) => scubaReservation);
                //todo: this should not be a hard coded url
                userScubaData.ChatWithUserUrl = "https://contososcubademo.azurewebsites.net?chatWithId=" + reserverReference.User.Id;
                //chatWithUserIdUrl = "Use this URL to chat with them: http://localhost:3979?chatWithId=" + reserverReference.User.Id;
            }
            string message = $"New scuba booking for {userScubaData.PersonalInfo.Name}";

            var replaceInfo = new Dictionary<string, string>();
            replaceInfo.Add("{{destination}}", userScubaData.Destination);
            replaceInfo.Add("{{school}}", userScubaData.School);
            replaceInfo.Add("{{longdate}}", Convert.ToDateTime(userScubaData.Date).ToString("dddd, MMMM dd"));
            replaceInfo.Add("{{number_of_people}}", userScubaData.NumberOfPeople);
            replaceInfo.Add("{{phone}}", userScubaData.PersonalInfo.Phone);
            replaceInfo.Add("{{email}}", userScubaData.PersonalInfo.Email);
            replaceInfo.Add("{{name}}", userScubaData.PersonalInfo.Name);
            replaceInfo.Add("{{protein_preference}}", userScubaData.MealOptions.ProteinPreference);
            replaceInfo.Add("{{vegan}}", userScubaData.MealOptions.Vegan ? "Yes" : "No");
            replaceInfo.Add("{{allergy}}", userScubaData.MealOptions.Alergy);

            if (!string.IsNullOrEmpty(userScubaData.ChatWithUserUrl))
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
          
        public static string GetUserName(string userId)
        {
            Tuple<ConversationReference, UserScubaData> foundReference;
            if (_recentReservations.TryGetValue(userId, out foundReference))
            {
                return foundReference.Item2.PersonalInfo.Name;
            }

            return string.Empty;
        }

        public static bool UserIsMessagingSubscriber(string userId)
        {
            return _subscriberToUser.ContainsKey(userId);
        }

        public static void RemoveUserConnectionToSubscriber(string userId)
        {
            ConversationReference reference = null;
            _subscriberToUser.TryRemove(userId, out reference);
        }

        public static async Task ForwardToReservationUser(string userId, IMessageActivity message, BotAdapter adapter, MicrosoftAppCredentials workingCredentials, ConversationReference contosoReference)
        {
            Tuple<ConversationReference, UserScubaData> foundReference;
            if (_recentReservations.TryGetValue(userId, out foundReference))
            {
                _subscriberToUser.AddOrUpdate(userId, contosoReference, (key, oldValue) => contosoReference);
                Func<ITurnContext, Task> conversationCallback = GetConversationCallback(message, workingCredentials);
                await adapter.ContinueConversation(foundReference.Item1.Bot.Id, foundReference.Item1, conversationCallback);
            }
        }

        public static async Task<bool> ForwardedToSubscriber(string userId, IMessageActivity message, BotAdapter adapter, MicrosoftAppCredentials workingCredentials)
        {
            ConversationReference foundReference = null;
            if (_subscriberToUser.TryGetValue(userId, out foundReference))
            {
                Func<ITurnContext, Task> conversationCallback = GetConversationCallback(message, workingCredentials);
                await adapter.ContinueConversation(foundReference.Bot.Id, foundReference, conversationCallback);
                return true;
            }

            return false;
        }

        #endregion Users

        private static Func<ITurnContext, Task> GetConversationCallback(IMessageActivity message, MicrosoftAppCredentials workingCredentials)
        {
            Func<ITurnContext, Task> conversationCallback = async (context) =>
            {
                FixContextCredentials(context, workingCredentials);

                context.Activity.SetReplyFields(message);

                await context.SendActivity(message);
            };

            return conversationCallback;
        }
        private static Func<ITurnContext, Task> GetConversationCallback(string text, MicrosoftAppCredentials workingCredentials, string fullMessageText = null)
        {
            Func<ITurnContext, Task> conversationCallback = async (context) =>
            {               
                FixContextCredentials(context, workingCredentials);

                 Activity reply = null;
                if (string.IsNullOrEmpty(fullMessageText))
                {
                    reply = context.Activity.CreateReply(text);
                }
                else
                {
                    reply = context.Activity.GetReplyFromText(fullMessageText);
                    reply.Text = text;
                }
                await context.SendActivity(reply);
            };

            return conversationCallback;
        }

        private static void FixContextCredentials(ITurnContext context, MicrosoftAppCredentials workingCredentials)
        {
            //TODO: bug in connector credentials (password is blank, and appid is bot id depending on channel that sent the message)
            var contextCredentials = ((MicrosoftAppCredentials)context.Services.Get<Microsoft.Bot.Connector.IConnectorClient>("Microsoft.Bot.Connector.IConnectorClient").Credentials);
            contextCredentials.MicrosoftAppId = workingCredentials.MicrosoftAppId;
            contextCredentials.MicrosoftAppPassword = workingCredentials.MicrosoftAppPassword;
        }
    }
}
