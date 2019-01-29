using System;
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
        /// Gets or sets the (potential) set of QnA Services used.
        /// Given there can be multiple QnA services used in a single bot,
        /// QnA is represented as a Dictionary. This is also modeled in the
        /// "appsettings.json" file since the elements are named (string).
        /// This sample only uses a single QnA instance.
        /// </summary>
        /// <value>
        /// A QnAMaker client instance created based on configuration in the appsettings.json file.
        /// </value>
        public Dictionary<string, QnAMaker> QnAServices { get; set; } = new Dictionary<string, QnAMaker>();

        /// <summary>
        /// Gets or sets the Luis recognizer service used.
        /// </summary>
        /// <value>
        /// A <see cref="LuisRecognizer"/> client instance created based on configuration in the appsettings.json file.
        /// </value>
        public LuisRecognizer LuisRecognizerService { get; set; }

        /// <summary>
        /// Gets or sets the list of luis intents.
        /// </summary>
        public List<string> Intents { get; set; }
    }
}
