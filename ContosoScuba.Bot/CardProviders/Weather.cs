using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
                && scubaData.PersonalInfo == null;
        }

        public override async Task<ScubaCardResult> GetCardResult(UserScubaData scubaData, JObject value, string messageText)
        {
            var info = new Models.PersonalInfo();           
            info.Name = value.Value<string>("firstlast");
            info.Email = value.Value<string>("email");
            info.Phone = value.Value<string>("phone");

            var error = GetErrorMessage(info);
            if (!string.IsNullOrEmpty(error))
                return new ScubaCardResult() { ErrorMessage = error };

            scubaData.PersonalInfo = info;

            return new ScubaCardResult() { CardText = await GetCardText(scubaData) };
        }

        private async Task<string> GetCardText(UserScubaData scubaData)
        {
            DateTime date = Convert.ToDateTime(scubaData.Date);

            //fake weather card generated here
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