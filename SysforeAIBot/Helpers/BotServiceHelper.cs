using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Configuration;
using SysforeAIBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysforeAIBot.Helpers
{
    public class BotServiceHelper
    {
        /// <summary>
        /// Initialize the bot's references to external services.
        ///
        /// For example, QnaMaker services are created here.
        /// These external services are configured
        /// using the <see cref="AppSettings"/> class (based on the contents of your "appsettings.json" file).
        /// </summary>
        /// <param name="appSettings"><see cref="AppSettings"/> object based on your "appsettings.json" file.</param>
        /// <returns>A <see cref="BotServices"/> representing client objects to access external services the bot uses.</returns>
        /// <seealso cref="AppSettings"/>
        /// <seealso cref="QnAMaker"/>
        /// <seealso cref="LuisService"/>
        public static BotServices InitBotServices(AppSettings appSettings)
        {
            var luisIntents = appSettings.luisIntents;
            var qnaServices = new Dictionary<string, QnAMaker>();
            LuisRecognizer luisRecignizerService = null;
            List<string> IntentsList = appSettings.luisIntents;
            //Prepare Luis service
            LuisService luisService = new LuisService()
            {
                AppId = appSettings.luisApp.appId,
                AuthoringKey = appSettings.luisApp.authoringKey,
                Id = appSettings.luisApp.id,
                Name = appSettings.luisApp.name,
                Region = appSettings.luisApp.region,
                SubscriptionKey = appSettings.luisApp.subscriptionKey,
                Type = appSettings.luisApp.type,
                Version = appSettings.luisApp.version
            };
            var luisApp = new LuisApplication(luisService.AppId, luisService.AuthoringKey, luisService.GetEndpoint());
            luisRecignizerService = new LuisRecognizer(luisApp);
            //Prepare QnA service
            foreach (var qna in appSettings.qnaServices)
            {
                var qnaEndpoint = new QnAMakerEndpoint()
                {
                    KnowledgeBaseId = qna.kbId,
                    EndpointKey = qna.endpointKey,
                    Host = qna.hostname,
                };
                var qnaMaker = new QnAMaker(qnaEndpoint);
                qnaServices.Add(qna.name, qnaMaker);
            }
            //return new BotServices(luisRecignizerService, qnaServices, Intents);
            return new BotServices()
            {
                Intents = IntentsList,
                LuisRecognizerService = luisRecignizerService,
                QnAServices = qnaServices,

            };
        }
    }
}
