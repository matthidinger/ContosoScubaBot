using ContosoScuba.Bot.CardProviders;
using ContosoScuba.Bot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Linq;

namespace ContosoScuba.Bot.Services
{
    public class ScubaCardService
    {
        public const string ScubaDataKey = "ScubaData";

        private static Lazy<List<CardProvider>> _cardHandlers = new Lazy<List<CardProvider>>(() =>
        {
            return Assembly.GetCallingAssembly().DefinedTypes
                .Where(t => (typeof(CardProvider) != t && typeof(CardProvider).IsAssignableFrom(t) && !t.IsAbstract))
                .Select(t => (CardProvider)Activator.CreateInstance(t))
                .ToList();
        });
        
        //todo: move this to its own class
        public class ScubaCardResult
        {
            public string ErrorMessage { get; set; }
            public string CardText { get; set; }
        }

        public async Task<ScubaCardResult> GetNextCardText(IDialogContext context, Activity activity)
        {
            var botdata = context.PrivateConversationData;
            UserScubaData userScubaData = null;
            if (botdata.ContainsKey(ScubaDataKey))            
                userScubaData = botdata.GetValue<UserScubaData>(ScubaDataKey);            
            
            string valuePath = string.Empty;
            var jObjectValue = activity.Value as Newtonsoft.Json.Linq.JObject;
            
            var cardProvider = _cardHandlers.Value.FirstOrDefault(c => c.ProvidesCard(userScubaData, jObjectValue, activity.Text));
            if (cardProvider != null)
            {
                var validation = cardProvider.IsValid();
                if (!string.IsNullOrEmpty(validation.ErrorMessage))
                {
                    return new ScubaCardResult() { ErrorMessage = "I'm sorry, I don't understand.  Please rephrase, or use the Adaptive Card to respond." };
                }
                else
                {
                    //for cards with single fields,
                    //users can enter chat text (OR interact with the card's controls)
                    string cardText = await cardProvider.GetCardText(userScubaData, jObjectValue, activity.Text);
                    if (userScubaData == null)
                        userScubaData = new UserScubaData();

                    botdata.SetValue<UserScubaData>(ScubaDataKey, userScubaData);
                    return new ScubaCardResult() { CardText = cardText };
                }
            }
            return null;
        }
        
        public static async Task<string> GetCardText(string cardName)
        {
            var path = HostingEnvironment.MapPath($"/Cards/{cardName}.JSON");
            if (!File.Exists(path))
                return string.Empty;

            using (var f = File.OpenText(path))
            {
                return await f.ReadToEndAsync();                
            }
        } 
    }
}