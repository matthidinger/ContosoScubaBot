using ContosoScuba.Bot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using System;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net.Http;

namespace ContosoScuba.Bot.Services
{
    public static class ReservationSubscriptionService
    {
        private static ConcurrentDictionary<string, ConversationReference> _reservationSubscribers = new ConcurrentDictionary<string, ConversationReference>();

        public static void AddOrUpdateReservation(string userId, ConversationReference conversationReference)
        {
            _reservationSubscribers.AddOrUpdate(userId, conversationReference, (key, oldValue) => conversationReference);
        }

        public static void RemoveReservation(string userId)
        {
            ConversationReference reference = null;
            _reservationSubscribers.TryRemove(userId, out reference);
        }

        public static async void SendActionableMessage(UserScubaData userScubaData)
        {
            var client = new HttpClient();
            var content = new Body
            await client.PostAsync("https://adaptivetestfunctions.azurewebsites.net/api/SendScubaEmail?code=4tRDT5xalBkFidaesDGNSg1xVRcU2HPh7Ar7Zsc8vpAXE8DdG9mzHg==", content);
            // 
            // TODO: call this function to create the actionable message


            //var myData = new MyDataObject();
            //myData.EventName = "Publish adaptive card schema";
            //myData.ProfileImage = "http://.....";

            //var card = AdaptiveCard.FromJson("thetemplate.json", myData);

            //return card.ToJson();


            //var card = new AdaptiveCard();
            //card.Body.Add(new TextBlock() { Text = "sdsds" });
            //return card.ToJson();
        }

        public static void NotifySubscribers(UserScubaData userScubaData, BotAdapter adapter, MicrosoftAppCredentials workingCredentials)
        {
            Func<ITurnContext, Task> conversationCallback = async (context) => 
                {
                    //TODO: bug in connector credentials (password is blank, and appid is bot id depending on channel that sent the message)
                    var contextCredentials = ((MicrosoftAppCredentials)context.Services.Get<Microsoft.Bot.Connector.IConnectorClient>("Microsoft.Bot.Connector.IConnectorClient").Credentials);
                    contextCredentials.MicrosoftAppId = workingCredentials.MicrosoftAppId;
                    contextCredentials.MicrosoftAppPassword = workingCredentials.MicrosoftAppPassword;

                    var notificationMessage = context.Activity.CreateReply($"New reservation for {userScubaData.PersonalInfo.Name} with {userScubaData.School} {userScubaData.Destination} on {userScubaData.Date}");
                    await context.SendActivity(notificationMessage);
                };

            foreach (var subscriber in _reservationSubscribers.Values)
            {
                adapter.ContinueConversation(subscriber.Bot.Id, subscriber, conversationCallback).RunSynchronously();
            }
        }
    }
}
