using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;

namespace ContosoScuba.Bot.CardProviders
{
    public class Lunch : CardProvider
    {
        public override string CardName => "5-Lunch";
        public override int CardIndex => 5;

        public override async Task<ScubaCardResult> GetCardResult(UserScubaData scubaData, JObject value, string messageText)
        {
            var date = value != null ? value.Value<string>("scheduleDate") : messageText;

            if (date.ToLower() != "back")
            {
                var error = GetErrorMessage(date);
                if (!string.IsNullOrEmpty(error))
                    return new ScubaCardResult() { ErrorMessage = error };

                scubaData.Date = date;
                scubaData.CurrentCardIndex = CardIndex + 1;
            }

            return new ScubaCardResult() { CardText = await base.GetCardText() };
        }

        private string GetErrorMessage(string userInput)
        {
            if (string.IsNullOrEmpty(userInput))
            {
                return "Please enter a valid date";
            }

            DateTime dateHolder;
            if (DateTime.TryParse(userInput, out dateHolder))
            {
                if (dateHolder > DateTime.Today)
                    return string.Empty;

                return "Please enter a valid date in the future";
            }
            else
            {
                return "Please enter a valid date";
            }
        }
    }
}