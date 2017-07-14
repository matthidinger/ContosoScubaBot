using System;

namespace ContosoScuba.Bot.Models
{
    [Serializable]
    public class UserScubaData
    {
        public string School { get; set; }
        public string Destination { get; set; }
        public string NumberOfPeople { get; set; }
        public string Date { get; set; }
        public MealOptions MealOptions { get; set; }
        public PersonalInfo PersonalInfo { get; set; }
    }

    [Serializable]
    public class MealOptions
    {
        public string ProteinPreference { get; set; }
        public bool Vegan { get; set; }
        public string Alergy { get; set; }
    }

    [Serializable]
    public class PersonalInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}