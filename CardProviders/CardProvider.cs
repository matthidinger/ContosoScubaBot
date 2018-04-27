using ContosoScuba.Bot.Models;
using ContosoScuba.Bot.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContosoScuba.Bot.CardProviders
{
    public abstract class CardProvider
    {
        public abstract string CardName { get; }

        public abstract bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText);

        public abstract Task<ScubaCardResult> GetCardResult(UserScubaData scubaData, JObject value, string messageText);

        protected async Task<string> GetCardText(Dictionary<string,string> replaceInfo = null)
        {
            string cardJson = await ScubaCardService.GetCardText(CardName);
            if (string.IsNullOrEmpty(cardJson))
                return string.Empty;

            if (replaceInfo==null)
                return cardJson;

            foreach (var replaceKvp in replaceInfo)            
                 cardJson = cardJson.Replace(replaceKvp.Key, replaceKvp.Value);
            
            return cardJson;
        }
    }
}