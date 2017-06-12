using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;

namespace ContosoScuba.Bot.CardProviders
{
    public class Locations : CardProvider
    {
        public override string CardName => "2-Locations";

        protected override string ReplaceText { get { return "{{school}}"; } }

        public override bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText)
        {
            return scubaData != null 
                && string.IsNullOrEmpty(scubaData.Location) 
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
            
            return base.GetCardText(scubaData.School);
        }
    }
}