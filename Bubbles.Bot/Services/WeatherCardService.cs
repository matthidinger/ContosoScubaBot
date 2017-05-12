using AdaptiveCards;
using APIXULib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Bubbles.Bot.Services
{
    public class WeatherService
    {
        public AdaptiveCard GetFakeAPIXUWeatherCard(string place, string date)
        {
            WeatherModel model = new Repository().GetWeatherData(ConfigurationManager.AppSettings["APIXUKey"], GetBy.CityName, place, Days.Five);

            var card = new AdaptiveCard();
            if (model != null)
            {
                if (model.current != null)
                {
                    card.Speak = $"<s>Here's the weather for your trip on {date}</s>";
                }

                if (model.forecast != null && model.forecast.forecastday != null)
                {
                    AddCurrentWeather(model, card, date);
                    AddForecast(place, model, card);

                    return card;
                }
            }
            return null;
        }

        public AdaptiveCard GetAPIXUWeatherCard(string place)
        {
            WeatherModel model = new Repository().GetWeatherData(ConfigurationManager.AppSettings["APIXUKey"], GetBy.CityName, place, Days.Five);

            var card = new AdaptiveCard();
            if (model != null)
            {
                if (model.current != null)
                {
                    card.Speak = $"<s>Today the temperature is {model.current.temp_f}</s><s>Winds are {model.current.wind_mph} miles per hour from the {model.current.wind_dir}</s>";
                }

                if (model.forecast != null && model.forecast.forecastday != null)
                {
                    AddCurrentWeather(model, card);
                    AddForecast(place, model, card);

                    return card;
                }
            }
            return null;
        }

        private static void AddCurrentWeather(WeatherModel model, AdaptiveCard card, string overrideDate = "")
        {
            var current = new ColumnSet();
            card.Body.Add(current);

            var currentColumn = new Column();
            current.Columns.Add(currentColumn);
            currentColumn.Size = "35";

            var currentImage = new Image();
            currentColumn.Items.Add(currentImage);
            currentImage.Url = model.current.condition.icon;

            var currentColumn2 = new Column();
            current.Columns.Add(currentColumn2);
            currentColumn2.Size = "65";

            string date = DateTime.Parse(model.current.last_updated).DayOfWeek.ToString();
            if (!string.IsNullOrEmpty(overrideDate))
                date = overrideDate;

            AddItem(currentColumn2, $"{model.location.name} ({date})", TextSize.Large, false);
            AddItem(currentColumn2, $"{model.current.temp_f.ToString().Split('.')[0]}° F", TextSize.Large);
            AddItem(currentColumn2, $"{model.current.condition.text}", TextSize.Medium);
            AddItem(currentColumn2, $"Winds {model.current.wind_mph} mph {model.current.wind_dir}", TextSize.Medium);
        }

        private static void AddForecast(string place, WeatherModel model, AdaptiveCard card)
        {
            var forecast = new ColumnSet();
            card.Body.Add(forecast);

            foreach (var day in model.forecast.forecastday)
            {
                if (DateTime.Parse(day.date).DayOfWeek != DateTime.Parse(model.current.last_updated).DayOfWeek)
                {
                    var column = new Column();
                    AddForcastColumn(forecast, column, place);
                    AddItem(column, DateTimeOffset.Parse(day.date).DayOfWeek.ToString().Substring(0, 3), TextSize.Medium);
                    AddImageColumn(day, column);
                    AddItem(column, $"{day.day.mintemp_f.ToString().Split('.')[0]}/{day.day.maxtemp_f.ToString().Split('.')[0]}", TextSize.Medium);
                    //AddItem(column, day.day.mintemp_f.ToString().Split('.')[0], TextSize.Medium);
                }
            }
        }

        private static void AddImageColumn(Forecastday day, Column column)
        {
            var image = new Image();
            image.Size = ImageSize.Auto;
            image.Url = day.day.condition.icon;
            column.Items.Add(image);
        }

        private static void AddForcastColumn(ColumnSet forecast, Column column, string place)
        {
            forecast.Columns.Add(column);
            column.Size = "20";
            var action = new OpenUrlAction();
            action.Url = $"https://www.bing.com/search?q=forecast in {place}";
            column.SelectAction = action;
        }

        private static void AddItem(Column column, string text, TextSize size, bool isSubTitle = true)
        {
            column.Items.Add(new TextBlock()
            {
                Text = text,
                Size = size,
                //Weight = TextWeight.Bolder,
                HorizontalAlignment = HorizontalAlignment.Center,
                IsSubtle = isSubTitle
            });
        }
    }
}