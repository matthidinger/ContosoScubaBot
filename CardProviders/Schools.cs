using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;

namespace ContosoScuba.Bot.CardProviders
{
    public class Schools : CardProvider
    {
        public override string CardName => "1-Schools";

        public override int CardIndex => 1;

        public override async Task<ScubaCardResult> GetCardResult(UserScubaData scubaData, JObject messageValue, string messageText)
        {
            scubaData.CurrentCardIndex = CardIndex + 1;
            return new ScubaCardResult() { CardText = await base.GetCardText() };
        }
    }
}