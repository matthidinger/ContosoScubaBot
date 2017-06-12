using ContosoScuba.Bot.Models;
using ContosoScuba.Bot.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace ContosoScuba.Bot.CardProviders
{
    public abstract class CardProvider 
    {
        protected virtual string ReplaceText { get { return string.Empty; } }

        public abstract string CardName { get; }

        public abstract bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText);

        public abstract Task<string> GetCardText(UserScubaData scubaData, JObject value, string messageText);

        protected async Task<string> GetCardText(string replaceWith = "")
        {
            string cardJson = await ScubaCardService.GetCardText(CardName);
            if (string.IsNullOrEmpty(cardJson))
                return string.Empty;

            if (string.IsNullOrEmpty(ReplaceText))
                return cardJson;

            return cardJson.Replace(ReplaceText, replaceWith);            
        }
    }
}