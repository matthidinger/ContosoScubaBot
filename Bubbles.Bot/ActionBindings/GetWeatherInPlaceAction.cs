using Bubbles.Bot.Services;
using Microsoft.Cognitive.LUIS.ActionBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bubbles.Bot.ActionBindings
{
    [Serializable]
    [LuisActionBinding("WeatherInPlace", FriendlyName = "Get the Weather in a location")]
    public class GetWeatherInPlaceAction : GetDataFromPlaceBaseAction
    {
        public override Task<object> FulfillAsync()
        {
            var current = new WeatherService().GetAPIXUWeatherCard(this.Place);
            return Task.FromResult((object)current);
        }
    }
}