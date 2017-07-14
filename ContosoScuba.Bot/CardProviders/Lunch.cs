using System;
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
                && string.IsNullOrEmpty(scubaData.Date)
                && (IsDate(messageText) || (value != null && value.First != null && value.First.Path == "date"));
            
            //todo: once date is being sent with the button click, validate that it is a real date
                //&& (IsDate(messageText) || (value != null && IsDate(value.Value<string>("scheduleDate"))));
        }

        public override Task<string> GetCardText(UserScubaData scubaData, JObject value, string messageText)
        {
            if (!string.IsNullOrEmpty(messageText))
                scubaData.Date = messageText;
            else
            {
                //todo: extract scheduleDate info from value (currently not being sent...bug)
                string date = value.Value<string>("scheduleDate");
                if (string.IsNullOrEmpty(date))
                {
                    date = DateTime.Now.AddDays(14).Date.ToString();
                }
                scubaData.Date = date;
            }
            return base.GetCardText();
        }
        static bool IsDate(string date)
        {
            DateTime Temp;

            if (DateTime.TryParse(date, out Temp) == true)
                return true;
            else
                return false;
        }
        
    }
}