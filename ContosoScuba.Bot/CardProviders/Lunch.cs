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
        
        public override bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText)
        {
            return scubaData != null
                && !string.IsNullOrEmpty(scubaData.NumberOfPeople)
                && string.IsNullOrEmpty(scubaData.Date);            
        }

        public override async Task<ScubaCardResult> GetCardResult(UserScubaData scubaData, JObject value, string messageText)
        {
            var date = value != null ? value.Value<string>("scheduleDate") : messageText;

            var error = GetErrorMessage(date);
            if (!string.IsNullOrEmpty(error))
                return new ScubaCardResult() { ErrorMessage = error };

            scubaData.Date = date;

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