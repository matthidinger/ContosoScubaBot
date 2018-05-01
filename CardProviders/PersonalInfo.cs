using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace ContosoScuba.Bot.CardProviders
{
    public class PersonalInfo : CardProvider
    {
        public override string CardName => "6-PersonalInfo";

        public override int CardIndex => 6;

        public override async Task<ScubaCardResult> GetCardResult(UserScubaData scubaData, JObject value, string messageText)
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
                mealOptions.ProteinPreference = messageText;

            if (messageText?.ToLower() != "back")
            {
                var error = GetErrorMessage(mealOptions.ProteinPreference);
                if (!string.IsNullOrEmpty(error))
                    return new ScubaCardResult() { ErrorMessage = error };

                scubaData.MealOptions = mealOptions;
                scubaData.CurrentCardIndex = CardIndex + 1;
            }

            return new ScubaCardResult() { CardText = await GetCardText() };
        }

        private string GetErrorMessage(string userInput)
        {
            if (string.IsNullOrEmpty(userInput))
            {
                return "Please enter Chicken, Beef, or Tofu.  You can also use the card to make your selection";
            }
            var lowered = userInput.ToLower();
            if (lowered == "tofu" || lowered == "chicken" || lowered == "beef")
            {
                return string.Empty;
            }
            else
            {
                return "Please enter Chicken, Beef, or Tofu.  You can also use the card to make your selection";
            }
        }
    }
}