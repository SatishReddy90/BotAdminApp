﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;

namespace SysforeAIBot
{
    /// <summary>
    /// Represents the bot's references to external services.
    ///
    /// For example, Application Insights, Luis models and QnaMaker services
    /// are kept here (singletons). These external services are configured
    /// using the BotConfigure class (based on the contents of your ".bot" file).
    /// </summary>
    public class BotServices
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotServices"/> class.
        /// </summary>
        /// <param name="qnaServices">A dictionary of named <see cref="QnAMaker"/> instances for usage within the bot.</param>
        /// <param name="luisRecognizer">A <see cref="LuisRecognizer"/> instance for usage within the bot</param>
        public BotServices(LuisRecognizer luisRecognizer, Dictionary<string, QnAMaker> qnaServices, List<string> Luis_Intents)
        {
            QnAServices = qnaServices ?? throw new ArgumentNullException(nameof(qnaServices));
            LuisRecognizerService = luisRecognizer;
            Intents = Luis_Intents;
        }

        /// <summary>
        /// Gets the (potential) set of QnA Services used.
        /// Given there can be multiple QnA services used in a single bot,
        /// QnA is represented as a Dictionary. This is also modeled in the
        /// ".bot" file since the elements are named (string).
        /// This sample only uses a single QnA instance.
        /// </summary>
        /// <value>
        /// A QnAMaker client instance created based on configuration in the .bot file.
        /// </value>
        public Dictionary<string, QnAMaker> QnAServices { get; } = new Dictionary<string, QnAMaker>();

        /// <summary>
        /// Gets the Luis recognizer service used.
        /// </summary>
        /// <value>
        /// A <see cref="LuisRecognizer"/> client instance created based on configuration in the .bot file.
        /// </value>
        public LuisRecognizer LuisRecognizerService { get; }

        public List<string> Intents { get; set; }
    }
}