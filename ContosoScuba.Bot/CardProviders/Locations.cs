using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ContosoScuba.Bot.CardProviders
{
    public class Locations : CardProvider
    {
        public override string CardName => "2-Locations";
        
        public override bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText)
        {
            return scubaData != null 
                && string.IsNullOrEmpty(scubaData.Destination) 
                && (IsSchool(messageText) || (value != null && IsSchool(value.Value<string>("school"))));
        }

        private bool IsSchool(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            var lowered = text.ToLower();
            return (lowered.Contains("adventure")
                    || lowered.Contains("relecloud")
                    || lowered.Contains("margie")
                    || lowered.Contains("fabrikam"));
        }

        public override Task<string> GetCardText(UserScubaData scubaData, JObject value, string messageText)
        {
            if (value != null)
                scubaData.School = value.Value<string>("school");
            else
                scubaData.School = messageText;

            var replaceInfo = new Dictionary<string, string>();
            replaceInfo.Add("{{school}}", scubaData.School);
            
            return base.GetCardText(replaceInfo);
        }
    }
}