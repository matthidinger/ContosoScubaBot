using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Connector;
using Microsoft.Cognitive.LUIS.ActionBinding.Bot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace Bubbles.Bot.Dialogs
{
    [Serializable]
    public class WeatherDialog : LuisActionDialog<object>
    {
        public WeatherDialog() : base(new Assembly[] { typeof(WeatherDialog).Assembly },
            (action, context) => { },
            new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["LUIS_ModelId"], ConfigurationManager.AppSettings["LUIS_SubscriptionKey"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntentActionResultHanlderAsync(IDialogContext context, object actionResult)
        {
            var message = context.MakeMessage();

            message.Text = "Please ask me about SCUBA diving or weather in a city.";

            await context.PostAsync(message);
        }

        [LuisIntent("WeatherInPlace")]
        public async Task WeatherInPlaceActionHandlerAsync(IDialogContext context, object actionResult)
        {
            IMessageActivity message = null;
            var weatherCard = (AdaptiveCards.AdaptiveCard)actionResult;
            if (weatherCard == null)
            {
                message = context.MakeMessage();
                message.Text = $"I couldn't find the weather for '{context.Activity.AsMessageActivity().Text}'.  Are you sure that's a real city?";
            }
            else
            {
                message = GetMessage(context, weatherCard, "Weather card");
            }

            await context.PostAsync(message);
        }

        private IMessageActivity GetMessage(IDialogContext context, AdaptiveCards.AdaptiveCard card, string cardName)
        {
            var message = context.MakeMessage();
            if (message.Attachments == null)
                message.Attachments = new List<Attachment>();

            var attachment = new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
                Name = $"Weather card"
            };
            message.Attachments.Add(attachment);
            return message;
        }

        //[LuisIntent("ScubaKnowledge")]
        //public async Task ScubaKnowledgeIntentActionResultHandlerAsync(IDialogContext context, object actionResult)
        //{
        //    var message = context.MakeMessage();
        //    message.Text = actionResult != null ? actionResult.ToString() : "Cannot resolve your query";
        //    await context.PostAsync(message);
        //}

        //[LuisIntent("FindScuba")]
        //public async Task ScubaIntentActionResultHandlerAsync(IDialogContext context, object actionResult)
        //{
        //    var reply = context.MakeMessage();

        //    var attachment = new Attachment()
        //    {
        //        Content = await new CardFinder().GetScubaCard("ScubaCard"),
        //        ContentType = "application/vnd.microsoft.card.adaptive",
        //        Name = $"card"
        //    };

        //    reply.Attachments.Add(attachment);
        //    reply.Text = "Here are some places near you:";

        //    await context.PostAsync(reply);
        //}


    }
}