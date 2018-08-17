using ContosoScuba.Bot.Models;
using System;

namespace ContosoScuba.Bot.Services
{
    public static class SampleData
    {
        public static T GetData<T>()
            where T : class
        {
            var type = typeof(T);
            
            if (type == typeof(WildlifeData))
                return GetSampleWildlifeData() as T;
            else if (type == typeof(UserScubaData))
                return GetSampleScubaData() as T;

            return default(T);
        }

        static UserScubaData GetSampleScubaData()
        {
            return new UserScubaData()
            {
                School = "Fabrikam",
                Destination = "Adventure Works",
                NumberOfPeople = "6",
                Date = DateTime.Now.AddDays(4).ToString(),
                MealOptions = new MealOptions()
                {
                    Alergy = "none",
                    ProteinPreference = "Beef",
                    Vegan = false
                },
                PersonalInfo = new PersonalInfo()
                {
                    Email = "customeremail@microsoft.com",
                    Name = "Customer Name",
                    Phone = "888.888.7000"
                }
            };
        }

        static WildlifeData GetSampleWildlifeData()
        {
            var wildlifeData = new WildlifeData();
            wildlifeData.Introduction = "You will be surprised how much amazingness you may see underwater.";
            wildlifeData.Instructions = "Click on each image to *dive deep*!";
            //todo: hard code sample image path
            string imageUrl = "http://contososcubademo.azurewebsites.net/assets/{0}.jpg";
            wildlifeData.Images.Add(new ViewImage() { ImageUrl = string.Format(imageUrl, "1"), OpenUrl = "https://www.bing.com/images/search?q=ocean+life" });
            wildlifeData.Images.Add(new ViewImage() { ImageUrl = string.Format(imageUrl, "2"), OpenUrl = "https://www.bing.com/images/search?q=ocean+life" });
            wildlifeData.Images.Add(new ViewImage() { ImageUrl = string.Format(imageUrl, "3"), OpenUrl = "https://www.bing.com/images/search?q=ocean+life" });
            wildlifeData.Images.Add(new ViewImage() { ImageUrl = string.Format(imageUrl, "4"), OpenUrl = "https://www.bing.com/images/search?q=ocean+life" });
            wildlifeData.Images.Add(new ViewImage() { ImageUrl = string.Format(imageUrl, "5"), OpenUrl = "https://www.bing.com/images/search?q=ocean+life" });
            return wildlifeData;
        }
    }

}