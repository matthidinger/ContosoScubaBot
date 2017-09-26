using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Connector;

namespace ContosoScuba.Bot.CardProviders
{
    public class Schools : CardProvider
    {
        public override string CardName => "1-Schools";

        public override bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText)
        {
            return scubaData == null;
        }

        public override async Task<ScubaCardResult> GetCardResult(Activity activity, UserScubaData scubaData, JObject messageValue, string messageText)
        {
            return new ScubaCardResult() { CardText = await base.GetCardText(activity) };
        }
    }
}