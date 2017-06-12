using System;
using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;

namespace ContosoScuba.Bot.CardProviders
{
    public class Date : CardProvider
    {
        public override string CardName => "4-Date";

        protected override string ReplaceText { get { return "{{number_of_people}}"; } }

        public override Task<string> GetCardText(UserScubaData scubaData, JObject value, string messageText)
        {
            if(value!=null)            
                scubaData.NumberOfPeople = value.Value<string>("numberOfPeople");            
            else
                scubaData.NumberOfPeople = messageText;

            return base.GetCardText(scubaData.NumberOfPeople);
        }

        public override bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText)
        {
            return scubaData!=null 
                && !string.IsNullOrEmpty(scubaData.Location) 
                && string.IsNullOrEmpty(scubaData.NumberOfPeople)
                && (IsNumeric(messageText) || (value!=null && IsNumeric(value.Value<string>("numberOfPeople"))));              
        }

        public static bool IsNumeric(string Expression)
        {
            double retNum;
            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }
    }
}