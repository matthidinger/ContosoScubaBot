using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ContosoScuba.Bot.CardProviders
{
    public class People : CardProvider
    {
        public override string CardName => "3-People";

        public override int CardIndex => 3;

        public override async Task<ScubaCardResult> GetCardResult(UserScubaData scubaData, JObject value, string messageText)
        {
            var destination = value != null ? value.Value<string>("destination") : messageText;

            if (destination.ToLower() != "back")
            {
                var error = GetErrorMessage(destination);
                if (!string.IsNullOrEmpty(error))
                    return new ScubaCardResult() { ErrorMessage = error };

                scubaData.CurrentCardIndex = CardIndex + 1;
                scubaData.Destination = destination;
            }

            return new ScubaCardResult() { CardText = await GetCardText(scubaData) };
        }

        private async Task<string> GetCardText(UserScubaData scubaData)
        {
            var replaceInfo = new Dictionary<string, string>();
            replaceInfo.Add("{{destination}}", scubaData.Destination);

            return await base.GetCardText(replaceInfo);
        }

        private string GetErrorMessage(string userInput)
        {
            var lowered = userInput.ToLower();
            if (lowered.Contains("alki")
                || lowered.Contains("golden")
                || lowered.Contains("island")
                || lowered.Contains("beach")
                || lowered.Contains("garden")
                || lowered.Contains("bainbridge"))
            {
                return string.Empty;
            }
            return "Please enter Alki Beach, Golden Gardens Park, or Bainbridge Island.  You can also click on the picture for your selection ";
        }
    }
}