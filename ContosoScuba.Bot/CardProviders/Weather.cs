using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
               // && scubaData.PersonalInfo == null
                && (value != null && value.First != null && value.First.Path == "personalInfo");
        }

        public override Task<string> GetCardText(UserScubaData scubaData, JObject value, string messageText)
        {
            var info = new Models.PersonalInfo();
            //todo: firstlast, email and phone are not currently sent by adaptive card (bug)
            info.Name = value.Value<string>("firstlast");
            info.Email = value.Value<string>("email");
            info.Phone = value.Value<string>("phone");
          
            scubaData.PersonalInfo = info;
            DateTime date = Convert.ToDateTime(scubaData.Date);

            var replaceInfo = new Dictionary<string, string>();
            replaceInfo.Add("{{name}}", scubaData.PersonalInfo.Name);
            replaceInfo.Add("{{email}}", scubaData.PersonalInfo.Email);
            replaceInfo.Add("{{phone}}", scubaData.PersonalInfo.Phone);
            replaceInfo.Add("{{school}}", scubaData.School);
            replaceInfo.Add("{{destination}}", scubaData.Destination);
            replaceInfo.Add("{{weekday}}", date.DayOfWeek.ToString());
            replaceInfo.Add("{{longdate}}", date.ToString("dddd, MMMM dd"));
            replaceInfo.Add("{{day1}}", date.AddDays(1).DayOfWeek.ToString().Substring(0,3));
            replaceInfo.Add("{{day2}}", date.AddDays(2).DayOfWeek.ToString().Substring(0, 3));
            replaceInfo.Add("{{day3}}", date.AddDays(3).DayOfWeek.ToString().Substring(0, 3));
            replaceInfo.Add("{{day4}}", date.AddDays(4).DayOfWeek.ToString().Substring(0, 3));

            return base.GetCardText(replaceInfo);
        }
    }
}