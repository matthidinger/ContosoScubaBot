using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace ContosoScuba.Bot.CardProviders
{
    public class Weather : CardProvider
    {
        public override string CardName => "7-Weather";
        
        public override bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText)
        {
            return scubaData != null
                && scubaData.MealOptions != null
                && scubaData.PersonalInfo == null
                && (value != null && value.First != null && value.First.Path == "personalInfo");
        }

        public override Task<string> GetCardText(UserScubaData scubaData, JObject value, string messageText)
        {
            //TODO: extract personal info from value (not currently being sent, not sure why)??
            var info = new Models.PersonalInfo();
            if (info != null)
            {
                info.Name = value.Value<string>("myName");
                info.Email = value.Value<string>("myEmail");
                info.Phone = value.Value<string>("myTel");
            }
            
            scubaData.PersonalInfo = info;
            return base.GetCardText();
        }
    }
}