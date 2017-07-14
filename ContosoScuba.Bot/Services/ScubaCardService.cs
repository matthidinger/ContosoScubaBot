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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public async Task<ScubaCardResult> GetNextCardText(IDialogContext context, Activity activity)
        {
            var botdata = context.PrivateConversationData;

            var userInput = activity.Text;
            UserScubaData userScubaData = null;
            if (botdata.ContainsKey(ScubaDataKey))
                userScubaData = botdata.GetValue<UserScubaData>(ScubaDataKey);

            var jObjectValue = activity.Value as Newtonsoft.Json.Linq.JObject;

            var cardProvider = _cardHandlers.Value.FirstOrDefault(c => c.ProvidesCard(userScubaData, jObjectValue, userInput));

            if (cardProvider != null)
            {
                //for cards with single fields,
                //users can enter chat text (OR interact with the card's controls)
                if (userScubaData == null)
                    userScubaData = new UserScubaData();

                var cardResult = await cardProvider.GetCardResult(userScubaData, jObjectValue, activity.Text);
                if(string.IsNullOrEmpty(cardResult.ErrorMessage))
                    botdata.SetValue<UserScubaData>(ScubaDataKey, userScubaData);

                return cardResult;
            }
            return new ScubaCardResult() { ErrorMessage = "I'm sorry, I don't understand.  Please rephrase, or use the Adaptive Card to respond." };
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

        //public bool IsValid(string cardNumber, string userInput, ValidationService validationService)
        //{
        //    return validationService.Validate(cardNumber, userInput);
        //}

        //public string SendErrorMessageText()
        //{
        //    return "This is an error message";
        //}
    }
}