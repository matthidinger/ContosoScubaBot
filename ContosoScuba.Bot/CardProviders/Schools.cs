using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;

namespace ContosoScuba.Bot.CardProviders
{
    public class Schools : CardProvider
    {
        public override string CardName => "1-Schools";

        public override bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText)
        {
            return scubaData == null;
        }

        public override async Task<ScubaCardResult> GetCardResult(UserScubaData scubaData, JObject messageValue, string messageText)
        {
            return new ScubaCardResult() { CardText = await base.GetCardText() };
        }
    }
}