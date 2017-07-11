using System;
using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ContosoScuba.Bot.CardProviders
{
    public class Date : CardProvider
    {
        public override string CardName => "4-Date";
        
        public override Task<string> GetCardText(UserScubaData scubaData, JObject value, string messageText)
        {
            if(value!=null)            
                scubaData.NumberOfPeople = value.Value<string>("numberOfPeople");            
            else
                scubaData.NumberOfPeople = messageText;

            var replaceInfo = new Dictionary<string, string>();
            replaceInfo.Add("{{number_of_people}}", scubaData.NumberOfPeople);

            return base.GetCardText(replaceInfo);
        }

        public override bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText)
        {
            return scubaData!=null 
                && !string.IsNullOrEmpty(scubaData.Destination) 
                && string.IsNullOrEmpty(scubaData.NumberOfPeople)
                && (IsNumeric(messageText) || (value!=null && IsNumeric(value.Value<string>("numberOfPeople"))));              
        }

        public static bool IsNumeric(string Expression)
        {
            double retNum;
            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        public override ValidationResult IsValid()
        {
            var result = new ValidationResult()
        }
    }
}