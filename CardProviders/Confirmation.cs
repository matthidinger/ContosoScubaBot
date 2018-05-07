using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ContosoScuba.Bot.CardProviders
{
    public class Confirmation : CardProvider
    {
        public override string CardName => "7-Confirmation";

        public override int CardIndex => 7;
        
        public override async Task<ScubaCardResult> GetCardResult(UserScubaData scubaData, JObject value, string messageText)
        {
            var info = new Models.PersonalInfo();

            if (value != null)
            {
                info.Name = GetValue(value, "firstlast");
                info.Email = GetValue(value, "email");
                info.Phone = GetValue(value, "phone");
            }

            var error = GetErrorMessage(info);
            if (!string.IsNullOrEmpty(error))
                return new ScubaCardResult() { ErrorMessage = error };

            scubaData.PersonalInfo = info;
            
            return new ScubaCardResult() { CardText = await GetCardText(scubaData), NotifySubscribers = true };
        }

        private string GetValue(JObject value, string valueName)
        {
            JToken token;
            if (value.TryGetValue(valueName, out token))
                return token.Value<string>();

            return string.Empty;
        }

        private async Task<string> GetCardText(UserScubaData scubaData)
        {
            DateTime date = Convert.ToDateTime(scubaData.Date);

            //fake receipt and weather cards generated here
            //in order to create a real weather card, 
            //see https://blog.botframework.com/2017/06/07/Adaptive-Card-Dotnet/
            var replaceInfo = new Dictionary<string, string>();
            replaceInfo.Add("{{name}}", scubaData.PersonalInfo.Name);
            replaceInfo.Add("{{email}}", scubaData.PersonalInfo.Email);
            replaceInfo.Add("{{phone}}", scubaData.PersonalInfo.Phone);
            replaceInfo.Add("{{school}}", scubaData.School);
            replaceInfo.Add("{{destination}}", scubaData.Destination);
            replaceInfo.Add("{{weekday}}", date.DayOfWeek.ToString());
            replaceInfo.Add("{{longdate}}", date.ToString("dddd, MMMM dd"));
            replaceInfo.Add("{{day1}}", date.AddDays(1).DayOfWeek.ToString().Substring(0, 3));
            replaceInfo.Add("{{day2}}", date.AddDays(2).DayOfWeek.ToString().Substring(0, 3));
            replaceInfo.Add("{{day3}}", date.AddDays(3).DayOfWeek.ToString().Substring(0, 3));
            replaceInfo.Add("{{day4}}", date.AddDays(4).DayOfWeek.ToString().Substring(0, 3));
            replaceInfo.Add("{{number_of_people}}", scubaData.NumberOfPeople);

            return await base.GetCardText(replaceInfo);
        }

        private string GetErrorMessage(Models.PersonalInfo personalInfo)
        {
            if (string.IsNullOrWhiteSpace(personalInfo.Name)
                || string.IsNullOrWhiteSpace(personalInfo.Email)
                || string.IsNullOrWhiteSpace(personalInfo.Phone))            
                return "Please fill out all the fields";
            
            if (!Regex.IsMatch(personalInfo.Email, @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"
                                         + "@"
                                         + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$"))            
                return "Please enter a valid email address";
            
            if (!Regex.IsMatch(personalInfo.Phone, @"\(?\d{3}\)?[-\.]? *\d{3}[-\.]? *[-\.]?\d{4}"))            
                return "Please enter a valid phone number";
            
            return string.Empty;
        }
    }
}