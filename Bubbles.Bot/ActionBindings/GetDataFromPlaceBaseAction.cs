using Microsoft.Cognitive.LUIS.ActionBinding;
using System;
using System.ComponentModel.DataAnnotations;
using Bubbles.Bot.Attributes;

namespace Bubbles.Bot.ActionBindings
{
    [Serializable]
    public abstract class GetDataFromPlaceBaseAction : BaseLuisAction
    {
        /// <summary>
        /// Location (Required)
        /// </summary>
        [Required(ErrorMessage = "Please provide a location")]
        [Location(ErrorMessage = "Please provide a valid location")]
        public string Place { get; set; }
    }
}