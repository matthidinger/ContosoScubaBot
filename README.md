# Contoso Dive Finder

## Introduction 

Most chat bot platforms have a single input box through which users type messages in order to interact with a bot.  [**Adaptive Cards**](http://adaptivecards.io/) provide a new way to create custom and interactive cards, with rich visuals and controls, that adapt to the platform on which they are being displayed.  This **Contoso Dive Finder** example demonstrates incorporating Adaptive Cards into a bot built using the [**Microsoft Bot Builder .NET SDK**](https://github.com/Microsoft/BotBuilder).  

![Walk THrough](WalkThrough.gif "Visual Walk Through")

## Getting Started

A published example of the **Contoso Dive Finder** is: [ContosoScuba.AzureWebsites.net](https://contososcuba.azurewebsites.net)  If you desire to run the sample yourself: 

* Download the source
* Restore Nuget Packages
* Register a bot on [https://dev.botframework.com/](https://dev.botframework.com/)
* Retrieve your bot's **Web Chat** channel secret from the dev portal and add it to the **default.htm** page
* Add your bot's **MicrosoftAppId** and **MicrosoftAppPassword** to the Web.config's `<appSettings>` 
* Publish your bot (make sure the messaging endpoint in the dev portal is set to the published url)

## More Information

- [AdaptiveCards.io](http://adaptivecards.io)
- [Open Source Repository](https://github.com/Microsoft/AdaptiveCards)