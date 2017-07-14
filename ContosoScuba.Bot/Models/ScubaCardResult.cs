using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace ContosoScuba.Bot.Models
{
    public class ScubaCardResult
    {
        public string ErrorMessage { get; set; }
        public string CardText { get; set; }
    }
}