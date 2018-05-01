using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ContosoScuba.Bot.CardProviders
{
    public class Locations : CardProvider
    {
        public override string CardName => "2-Locations";

        public override int CardIndex => 2;
        
        public override async Task<ScubaCardResult> GetCardResult(UserScubaData scubaData, JObject value, string messageText)
        {
            var school = value != null ? value.Value<string>("school") : messageText;

            if (school.ToLower() != "back")
            {
                var error = GetErrorMessage(school);
                if (!string.IsNullOrEmpty(error))
                    return new ScubaCardResult() { ErrorMessage = error };

                scubaData.School = school;
                scubaData.CurrentCardIndex = CardIndex + 1;
            }
            return new ScubaCardResult() { CardText = await GetCardText(scubaData) };
        }

        private async Task<string> GetCardText(UserScubaData scubaData)
        {
            var replaceInfo = new Dictionary<string, string>();
            replaceInfo.Add("{{school}}", scubaData.School);

            return await base.GetCardText(replaceInfo);
        }

        private string GetErrorMessage(string userInput)
        {
            if (IsSchool(userInput))
            {
                return string.Empty;
            }
            return "Please enter Fabrikam, Margie\'s Travel, Relecloud Diving, or Adventure Works. You can also click the card button to make your selection.";
        }

        private bool IsSchool(string userInput)
        {
            if (string.IsNullOrEmpty(userInput))
                return false;

            var lowered = userInput.ToLower();
            return (lowered.Contains("adventure")
                || lowered.Contains("relecloud")
                || lowered.Contains("margie")
                || lowered.Contains("fabrikam"));
        }
    }
}