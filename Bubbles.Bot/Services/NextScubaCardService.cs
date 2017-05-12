using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace Bubbles.Bot.Services
{
    public class NextScubaCardService
    {

        public async Task<string> GetNextCardText(IDialogContext context, Activity activity)
        {
            CardTransitionInfo transitionInfo = GetCardTransitionInfo(context, activity);
            if (transitionInfo == null)
            {
                return GetFakeWeatherCard(context, activity);
            }
            return await GetCardText(transitionInfo);
        }
        private string GetFakeWeatherCard(IDialogContext context, Activity activity)
        {
            var botdata = context.ConversationData;
            var userScubaData = botdata.GetValue<UserScubaData>("ScubaData");

            var weatherCard = new WeatherService().GetFakeAPIXUWeatherCard(userScubaData.Destination, userScubaData.Date);

            var reply = activity.CreateReply("Great, we'll keep that in mind when getting your lunch!  That's all the info we needed, we'll see you soon! Here is a forecast of the weather for your trip");
            if (reply.Attachments == null)
                reply.Attachments = new List<Attachment>();

            //var confirmTransitionInfo = new CardTransitionInfo();
            //confirmTransitionInfo.NextCardName = "6-bf";
            //var confirmationCard = await GetCardText(confirmTransitionInfo);

            //var confirmationAttachment = new Attachment()
            //{
            //    Content = confirmationCard,
            //    ContentType = "application/vnd.microsoft.card.adaptive",
            //    Name = $"confirmation card"
            //};
            //reply.Attachments.Add(confirmationAttachment);
            var weatherAttachment = new Attachment()
            {
                Content = weatherCard,
                ContentType = "application/vnd.microsoft.card.adaptive",
                Name = $"weather card"
            };
            reply.Attachments.Add(weatherAttachment);

            var serialized = JsonConvert.SerializeObject(reply);
            return serialized;
        }
        CardTransitionInfo GetCardTransitionInfo(IDialogContext context, Activity activity)
        {
            

            var returnValue = new CardTransitionInfo();
            var text = activity.Text;
            returnValue.NextCardName = "1-bf";
            var botdata = context.ConversationData;
            UserScubaData userScubaData = null;
            if (botdata.ContainsKey(ScubaDataKey))
            {
                userScubaData = botdata.GetValue<UserScubaData>(ScubaDataKey);
            }

            var value = activity.Value as Newtonsoft.Json.Linq.JObject;
            bool missingValue = (value == null || value.First == null);

            if (missingValue && (userScubaData != null 
                && string.IsNullOrEmpty(userScubaData.Date) 
                && !string.IsNullOrEmpty(userScubaData.NumberOfPeople)
                && !string.IsNullOrEmpty(text) 
                && IsMealOption(text) 
                && string.IsNullOrEmpty(userScubaData.Date)))
            {
                //TEMP: couldn't get date to come back from emulator...not sure why..?                
                returnValue.NextCardName = "5-bf";
                userScubaData.Date = "6/1/2017";
                return returnValue;
            }
            if (!missingValue || userScubaData != null)
            {
                Tuple<string,string> switchStep =null;
                if (!missingValue)
                {
                    switchStep = new Tuple<string, string>( value.First.Path,value.Value<string>(value.First.Path));
                }
                else
                {
                    switchStep = GetSwitchStepFromText(text);
                }

                switch (switchStep.Item1)
                {
                    case "school":

                        //init userscubadata
                        userScubaData = new UserScubaData();
                        botdata.SetValue<UserScubaData>(ScubaDataKey, userScubaData);

                        returnValue.NextCardName = "2-bf";
                        returnValue.ReplaceText = "{{school_from_last_response}}";
                        userScubaData.School = switchStep.Item2;// value.Value<string>("school");
                        returnValue.ReplaceWithText = userScubaData.School;
                        break;

                    case "destination":

                        returnValue.NextCardName = "3-bf";
                        returnValue.ReplaceText = "{{previous_destination}}";
                        userScubaData.Destination = switchStep.Item2;// value.Value<string>("destination");
                        returnValue.ReplaceWithText = userScubaData.Destination;
                        break;

                    case "numberOfPeople":
                        returnValue.NextCardName = "4-bf";
                        returnValue.ReplaceText = "{{number_of_people}}";
                        userScubaData.NumberOfPeople = switchStep.Item2;// value.Value<string>("numberOfPeople");
                        returnValue.ReplaceWithText = userScubaData.NumberOfPeople;
                        break;


                    case "date":
                        returnValue.NextCardName = "5-bf";
                        string date = switchStep.Item2;// value.Value<DateTime>("date").ToString();
                        userScubaData.Date = date;
                        break;

                    case "mealOptions":
                        // returnValue.NextCardName = "6-bf";
                        return null;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                userScubaData = new UserScubaData();
            }
            botdata.SetValue<UserScubaData>(ScubaDataKey, userScubaData);
            return returnValue;
        }
        public const string ScubaDataKey = "ScubaData";
        Tuple<string,string> GetSwitchStepFromText(string text)
        {
            Tuple<string, string> step = null;
            if (!string.IsNullOrEmpty(text))
            {
                string lowerText = text.ToLower();
                if (lowerText.Contains("adventure") 
                    || lowerText.Contains("relecloud") 
                    || lowerText.Contains("margie") 
                    || lowerText.Contains("fabrikam"))
                {
                    step = new Tuple<string, string>("school", text);
                }
                else if (lowerText.Contains("alki") 
                    || lowerText.Contains("golden")
                   || lowerText.Contains("island")
                   || lowerText.Contains("beach")
                   || lowerText.Contains("garden")
                   || lowerText.Contains("bainbridge"))
                {
                    step = new Tuple<string, string>("destination", text);
                }
                else if (IsNumeric(text))
                {
                    step= new Tuple<string, string>("numberOfPeople", text);
                }
                else if(IsDate(text))
                {
                    step = new Tuple<string, string>("date", text);
                }
                else if (IsMealOption(text))
                {
                    step = new Tuple<string, string>("mealOptions", text);
                }
            }
            else
            {
                step = new Tuple<string, string>("date", text);
            }
            return step;
        }
        protected bool IsDate(string date)
        {
            DateTime Temp;
            
            if (DateTime.TryParse(date, out Temp) == true)
                return true;
            else
                return false;
        }
        static bool IsMealOption(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                string lowerText = text.ToLower();
                return lowerText.Contains("vegetarian")
                    || lowerText.Contains("vegan")
                    || lowerText.Contains("gluten")
                    || lowerText.Contains("peanut");
            }
            return false;
        }
        public static bool IsNumeric(string Expression)
        {
            double retNum;
            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }
        public async Task<string> GetCardText(CardTransitionInfo transitionInfo)
        {
            var path = HostingEnvironment.MapPath($"/Cards/{transitionInfo.NextCardName}.json");
            if (!File.Exists(path))
                return string.Empty;

            using (var f = File.OpenText(path))
            {
                string json = await f.ReadToEndAsync();

                if (string.IsNullOrEmpty(transitionInfo.ReplaceText))
                    return json;

                return json.Replace(transitionInfo.ReplaceText, transitionInfo.ReplaceWithText);
            }
        }
        [Serializable]
        public class UserScubaData
        {
            public UserScubaData()
            {
                Date = string.Empty;
                Destination = string.Empty;
            }
            public string School { get; set; }
            public string Destination { get; set; }
            public string NumberOfPeople { get; set; }
            public string Date { get; set; }
            public string MealOptions { get; set; }

        }
        public class CardTransitionInfo
        {
            public string NextCardName { get; set; }
            public string ReplaceText { get; set; }
            public string ReplaceWithText { get; set; }
        }
    }
}