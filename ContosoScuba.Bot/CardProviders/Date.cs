using System;
using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Bot.Connector;

namespace ContosoScuba.Bot.CardProviders
{
    public class Date : CardProvider
    {
        public override string CardName => "4-Date";

        public override bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText)
        {
            return scubaData != null
                && !string.IsNullOrEmpty(scubaData.Destination)
                && string.IsNullOrEmpty(scubaData.NumberOfPeople);
        }

        public override async Task<ScubaCardResult> GetCardResult(UserScubaData scubaData, JObject value, string messageText)
        {
            var numberOfPeople = value != null ? value.Value<string>("numberOfPeople") : messageText;

            var error = GetErrorMessage(numberOfPeople);
            if (!string.IsNullOrEmpty(error))
                return new ScubaCardResult() { ErrorMessage = error };

            scubaData.NumberOfPeople = numberOfPeople;

            return new ScubaCardResult() { CardText = await GetCardText(scubaData) };
        }

        private async Task<string> GetCardText(UserScubaData scubaData)
        {
            var replaceInfo = new Dictionary<string, string>();
            replaceInfo.Add("{{number_of_people}}", scubaData.NumberOfPeople);

            return await base.GetCardText(replaceInfo);
        }

        private string GetErrorMessage(string userInput)
        {
            double retNum;

            if ((Double.TryParse(Convert.ToString(userInput), System.Globalization.NumberStyles.Any,
                    System.Globalization.NumberFormatInfo.InvariantInfo, out retNum))
                && retNum <= 6
                && retNum >= 3)
            {
                return string.Empty;
            }

            return "Please enter a number between 3 and 6";            
        }
    }
}