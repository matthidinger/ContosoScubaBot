using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace ContosoScuba.Bot.CardProviders
{
    public class PersonalInfo : CardProvider
    {
        public override string CardName => "6-PersonalInfo";
        
        public override bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText)
        {
            return scubaData != null
                && !string.IsNullOrEmpty(scubaData.Date)
                && scubaData.MealOptions == null
                && (IsMealOptions(messageText) || (value != null && IsMealOptions(value.Value<string>("mealOptions"))));
        }

        private bool IsMealOptions(string mealOptions)
        {
            if (string.IsNullOrEmpty(mealOptions))
                return false;

            var lowered = mealOptions.ToLower();
            return (lowered == "tofu" || lowered == "chicken" || lowered == "beef");
        }

        public override Task<string> GetCardText(UserScubaData scubaData, JObject value, string messageText)
        {
            var mealOptions = new MealOptions();
            if (value != null)
            {
                mealOptions.ProteinPreference = value.Value<string>("mealOptions");
                if (mealOptions.ProteinPreference == "tofu")
                {
                    mealOptions.Vegan = value.Value<string>("Vegetarian") == "vegan";
                }

                mealOptions.Alergy = value.Value<string>("Allergy");
            }
            else
            {
                mealOptions.ProteinPreference = messageText;
            }
            scubaData.MealOptions = mealOptions;

            return base.GetCardText();
        }        
    }
}