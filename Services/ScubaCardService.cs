using ContosoScuba.Bot.CardProviders;
using ContosoScuba.Bot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Core.Extensions;

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
                .OrderBy(t=>t.CardIndex)
                .ToList();
        });

        public async Task<ScubaCardResult> GetNextCardText(ITurnContext context, Activity activity)
        {
            var userScubaData = context.GetConversationState<UserScubaData>();

            var userInput = activity.Text;

            var jObjectValue = activity.Value as Newtonsoft.Json.Linq.JObject;

            var cardProvider = _cardHandlers.Value.FirstOrDefault(c => c.ProvidesCard(userScubaData, jObjectValue, userInput));

            if (cardProvider != null)
            {
                return await cardProvider.GetCardResult(userScubaData, jObjectValue, activity.Text);
            }
            return new ScubaCardResult() { ErrorMessage = "I'm sorry, I don't understand.  Please rephrase, or use the Adaptive Card to respond." };
        }

        public static async Task<string> GetCardText(string cardName)
        {
            return await File.ReadAllTextAsync($@".\Cards/{cardName}.JSON");
        }
    }
}