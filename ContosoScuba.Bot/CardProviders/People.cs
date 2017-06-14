using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ContosoScuba.Bot.CardProviders
{
    public class People : CardProvider
    {
        public override string CardName => "3-People";
        
        public override Task<string> GetCardText(UserScubaData scubaData, JObject value, string messageText)
        {
            if (!string.IsNullOrEmpty(messageText))
                scubaData.Destination = messageText;
            else
                scubaData.Destination = value.Value<string>("destination");

            var replaceInfo = new Dictionary<string, string>();
            replaceInfo.Add("{{destination}}", scubaData.Destination);

            return base.GetCardText(replaceInfo);
        }

        public override bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText)
        {
            return scubaData != null
                    && !string.IsNullOrEmpty(scubaData.School)
                    && string.IsNullOrEmpty(scubaData.Destination)
                    && (IsLocation(messageText) || (value != null && IsLocation(value.Value<string>("destination"))));
        }

        private bool IsLocation(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            var lowered = text.ToLower();
            return (lowered.Contains("alki")
                    || lowered.Contains("golden")
                    || lowered.Contains("island")
                    || lowered.Contains("beach")
                    || lowered.Contains("garden")
                    || lowered.Contains("bainbridge"));
        }
    }
}