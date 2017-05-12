using Microsoft.Bot.Connector;
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

        public async Task<string> GetNextCardText(Activity activity)
        {
            CardTransitionInfo transitionInfo = GetCardTransitionInfo(activity);
            return await GetCardText(transitionInfo);
        }

        CardTransitionInfo GetCardTransitionInfo(Activity activity)
        {
            var returnValue = new CardTransitionInfo();
            var text = activity.Text;
            returnValue.NextCardName = "1-bf";

            var value = activity.Value as Newtonsoft.Json.Linq.JObject;
            if (value != null)
            {
                if (value.First == null)
                {
                  
                }
                else
                {
                    switch (value.First.Path)
                    {

                        case "school":
                            returnValue.NextCardName = "2-bf";
                            returnValue.ReplaceText = "{{school_from_last_response}}";
                            returnValue.ReplaceWithText = value.Value<string>("school");
                            break;

                        case "destination":
                            returnValue.NextCardName = "3-bf";
                            returnValue.ReplaceText = "{{previous_destination}}";
                            returnValue.ReplaceWithText = value.Value<string>("destination");
                            break;

                        case "numberOfPeople":
                            returnValue.NextCardName = "4-bf";
                            returnValue.ReplaceText = "{{number_of_people}}";
                            returnValue.ReplaceWithText = value.Value<string>("numberOfPeople");
                            break;


                        case "date":
                            returnValue.NextCardName = "5-bf";
                            //returnValue.ReplaceText = "{{previous_destination}}";
                            //returnValue.ReplaceWithText = value.Value<DateTime>("date");
                            break;

                        case "mealOptions":
                            returnValue.NextCardName = "6-bf";
                            break;

                        default:
                            break;
                    }
                }
            }
            return returnValue;
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

        public class CardTransitionInfo
        {
            public string NextCardName { get; set; }
            public string ReplaceText { get; set; }
            public string ReplaceWithText { get; set; }
        }
    }
}