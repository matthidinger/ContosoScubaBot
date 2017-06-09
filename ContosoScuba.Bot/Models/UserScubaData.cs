using System;

namespace ContosoScuba.Bot.Models
{
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
}