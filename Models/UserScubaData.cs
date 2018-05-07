using System;
using System.Collections.Generic;

namespace ContosoScuba.Bot.Models
{
    [Serializable]
    public class UserScubaData : Dictionary<string, object>
    {
        public UserScubaData()
        {

        }
        public int CurrentCardIndex
        {
            get { return ContainsKey(nameof(CurrentCardIndex)) ? Convert.ToInt32(base[nameof(CurrentCardIndex)]) : 1; }
            set { base[nameof(CurrentCardIndex)] = value.ToString(); }
        }

        public string School
        {
            get { return ContainsKey(nameof(School)) ? base[nameof(School)] as string : string.Empty; }
            set { base[nameof(School)] = value; }
        }
        public string Destination
        {
            get { return ContainsKey(nameof(Destination)) ? base[nameof(Destination)] as string : string.Empty; }
            set { base[nameof(Destination)] = value; }
        }
        public string NumberOfPeople
        {
            get { return ContainsKey(nameof(NumberOfPeople)) ? base[nameof(NumberOfPeople)] as string : string.Empty; }
            set { base[nameof(NumberOfPeople)] = value; }
        }
        public string Date
        {
            get { return ContainsKey(nameof(Date)) ? base[nameof(Date)] as string : string.Empty; }
            set { base[nameof(Date)] = value; }
        }
        public MealOptions MealOptions
        {
            get { return ContainsKey(nameof(MealOptions)) ? base[nameof(MealOptions)] as MealOptions : null; }
            set { base[nameof(MealOptions)] = value; }
        }
        public PersonalInfo PersonalInfo
        {
            get { return ContainsKey(nameof(PersonalInfo)) ? base[nameof(PersonalInfo)] as PersonalInfo : null; }
            set { base[nameof(PersonalInfo)] = value; }
        }

        public string ChatWithUserUrl
        {
            get { return ContainsKey(nameof(ChatWithUserUrl)) ? base[nameof(ChatWithUserUrl)] as string : string.Empty; }
            set { base[nameof(ChatWithUserUrl)] = value; }
        }
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