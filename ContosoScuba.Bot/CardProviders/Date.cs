using System;
using System.Threading.Tasks;
using ContosoScuba.Bot.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Bot.Connector;
using AdaptiveCards;
using Newtonsoft.Json;

namespace ContosoScuba.Bot.CardProviders
{
    public class Date : CardProvider
    {
        public override string CardName => "4-Date";

        public override bool ProvidesCard(UserScubaData scubaData, JObject value, string messageText)
        {
            return scubaData != null
                && ((!string.IsNullOrEmpty(scubaData.Destination)
                && string.IsNullOrEmpty(scubaData.NumberOfPeople)) || value["previousDate"] != null);
        }

        public override async Task<ScubaCardResult> GetCardResult(Activity activity, UserScubaData scubaData, JObject value, string messageText)
        {
            var previousDate = value == null ? null : value["previousDate"];
            if (previousDate != null)
            {
                return new ScubaCardResult() { CardText = await GetCardText(activity, scubaData, previousDate.Value<DateTime>() )};
            }
            else
            {
                var numberOfPeople = value != null ? value.Value<string>("numberOfPeople") : messageText;

                var error = GetErrorMessage(numberOfPeople);
                if (!string.IsNullOrEmpty(error))
                    return new ScubaCardResult() { ErrorMessage = error };

                scubaData.NumberOfPeople = numberOfPeople;

                return new ScubaCardResult() { CardText = await GetCardText(activity, scubaData) };
            }
        }

        private async Task<string> GetCardText(Activity activity, UserScubaData scubaData, DateTime? previousDate = null)
        {
            var cardText = await base.GetCardText(activity, new Dictionary<string, string>());
            
            var useMonth = DateTime.Now;
            if (previousDate.HasValue)            
                useMonth = previousDate.Value.AddMonths(1);
            
            //if there are only three days left in the month, use next month
            if (useMonth.AddDays(3).Month > useMonth.Month)
                useMonth = DateTime.Now.AddDays(3);
            var month = new Month(useMonth);

            var reply = Newtonsoft.Json.JsonConvert.DeserializeObject<Activity>(cardText);
            var card = month.ToCard();
            if (!previousDate.HasValue)
            {
                card.Speak = "<s>What date did you want to go?</s>";
                reply.Text = $"Excellent, {scubaData.NumberOfPeople} of your best buds!";
            }
            card.Body.Insert(0, new TextBlock() { Text = "What date would you like to go diving?" });
            reply.Attachments[0].Content = card;
            
            var replyAsString = Newtonsoft.Json.JsonConvert.SerializeObject(reply);
            return replyAsString;
        }

        private string GetErrorMessage(string userInput)
        {
            double retNum;

            if ((Double.TryParse(Convert.ToString(userInput), System.Globalization.NumberStyles.Any,
                    System.Globalization.NumberFormatInfo.InvariantInfo, out retNum))
                && retNum <= 6
                && retNum >= 3)
            {
                return string.Empty;
            }

            return "Please enter a number between 3 and 6";            
        }
    }

    class Day
    {
        public enum CalendarStatus
        {
            Available,
            Unavailable,
            Proposed
        }
        public DateTime Date { get; set; }
        public CalendarStatus Status { get; set; }

        public Day(DateTime date, DateTime firstDayOfMonth)
        {
            this.Date = date;
            if (date.DayOfWeek == DayOfWeek.Sunday || date.Date < DateTime.Now || date.Month != firstDayOfMonth.Month)
                this.Status = CalendarStatus.Unavailable;
            else
                this.Status = CalendarStatus.Available;
        }
    }

    class Month
    {
        public Day[] days;
        private string monthName;
        private string[] abbreviatedDayNames = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" }; //localization fail
        private DateTime firstDay;

        public Month(DateTime month)
        {
            days = new Day[7 * 6];
            var actualMonth = month.Month;
            var actualYear = month.Year;
            firstDay = new DateTime(actualYear, actualMonth, 1);
            this.monthName = month.ToString("MMMM");
            var first = firstDay.AddDays(0 - firstDay.DayOfWeek); //Find the first Sunday
            for (DateTime i = first; i < first.AddDays(7 * 6); i = i.AddDays(1))
            {
                days[(int)(i - first).TotalDays] = new Day(i, firstDay);
            }
        }

        public AdaptiveCard ToCard()
        {
            AdaptiveCard json = new AdaptiveCard();
            var monthContainer = new Container();
            monthContainer.Style = ContainerStyle.Normal;
            monthContainer.Items.Add(new TextBlock()
            {
                Text = this.monthName + " →",
                Weight = TextWeight.Bolder
            });
            var submitJson = JsonConvert.SerializeObject(new { PreviousDate = this.firstDay });
            monthContainer.SelectAction = new SubmitAction() { DataJson = submitJson };
            json.Body.Add(monthContainer);
            json.Body.Add(new ColumnSet());
            ColumnSet columnSet = (ColumnSet)json.Body[1];
            for (int cols = 0; cols < 7; cols++)
            {
                columnSet.Columns.Add(new Column());
                for (int rows = 0; rows < 7; rows++)
                {
                    var row = new TextBlock();
                    
                    if (rows == 0)
                    {
                        columnSet.Columns[cols].Items.Add(row);
                        row.Text = "**" + abbreviatedDayNames[cols] + "**";
                    }
                    else
                    {
                        var day = days[(rows - 1) * 7 + cols];
                        
                        row.Text = day.Date.Day.ToString();
                        
                        if (day.Status == Day.CalendarStatus.Proposed)
                        {
                            columnSet.Columns[cols].Items.Add(row);
                            row.Color = TextColor.Accent;
                        }
                        else if (day.Status == Day.CalendarStatus.Unavailable)
                        {
                            columnSet.Columns[cols].Items.Add(row);
                            row.Color = TextColor.Warning;
                        }
                        else if (day.Status == Day.CalendarStatus.Available)
                        {
                            var container = new Container();
                            container.Style = ContainerStyle.Normal;
                            container.Items.Add(row);

                            columnSet.Columns[cols].Items.Add(container);
                            container.SelectAction = new SubmitAction() { DataJson = JsonConvert.SerializeObject(new { scheduleDate = day.Date } )  };
                            row.Color = TextColor.Good;                        
                        }
                    }

                }
            }
            return json;
        }
    }

}