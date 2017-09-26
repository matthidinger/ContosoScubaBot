using ContosoScuba.Bot.Models;
using ContosoScuba.Bot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContosoScuba.Bot.CardProviders
{
    public abstract class CardProvider
    {
        public abstract string CardName { get; }

        public abstract bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText);

        public abstract Task<ScubaCardResult> GetCardResult(Activity activity, UserScubaData scubaData, JObject value, string messageText);

        protected async Task<string> GetCardText(Activity activity, Dictionary<string, string> replaceInfo = null)
        {
            string cardJson = await ScubaCardService.GetCardText(CardName, activity.ChannelId);
            if (string.IsNullOrEmpty(cardJson))
                return string.Empty;

            if (activity.ChannelId == ChannelIds.Kik)            
                cardJson = ScubaCardService.ReplaceKickText(cardJson, activity);
            
            if (replaceInfo == null)
                return cardJson;

            foreach (var replaceKvp in replaceInfo)
                cardJson = cardJson.Replace(replaceKvp.Key, replaceKvp.Value);


            return cardJson;
        }
    }
}