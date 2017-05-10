using System.ComponentModel.DataAnnotations;

namespace Bubbles.Bot.Attributes
{
    public class LocationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            // TODO: Actually validate location using Bing or something
            return value == null || ((string)value).Length >= 3;
        }
    }
}