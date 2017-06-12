using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;

namespace ContosoScuba.Bot.CardProviders
{
    public class People : CardProvider
    {
        public override string CardName => "3-People";

        protected override string ReplaceText { get { return "{{destination}}"; } }

        public override Task<string> GetCardText(UserScubaData scubaData, JObject value, string messageText)
        {
            if (!string.IsNullOrEmpty(messageText))
                scubaData.Location = messageText;
            else
                scubaData.Location = value.Value<string>("destination");

            return base.GetCardText(scubaData.Location);
        }

        public override bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText)
        {
            return scubaData != null
                    && !string.IsNullOrEmpty(scubaData.School)
                    && string.IsNullOrEmpty(scubaData.Location)
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