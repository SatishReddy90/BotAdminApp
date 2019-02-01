using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using SysforeAIBot.Models;
using SysforeAIBot.DTO;
using SysforeAIBot.NlpDispatch;
using System.Text.RegularExpressions;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using System.IO;
using Google.Apis.Util.Store;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Options;
using SysforeAIBot.Helpers;

namespace SysforeAIBot
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each interaction from the user, an instance of this class is called.
    /// This is a Transient lifetime service. Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single Turn, should be carefully managed.
    /// </summary>
    public class SysforeAIBotBot : IBot
    {
        private const string WelcomeText = "How may I help you today?";

        /// <summary>
        /// Services configured from the "appsettings.json" file.
        /// </summary>
        public BotServices _services { get; set; }
        /// <summary>
        /// Application related settings configured in "appsettings.json" file.
        /// </summary>
        public AppSettings _appSettings { get; set; }

        public DialogFlow _dialogFlow { get; set; }

        private readonly SysforeAIBotAccessors _accessors;

        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Google Sheets API .NET Quickstart";
        /// <summary>
        /// Initializes a new instance of the <see cref="NlpDispatchBot"/> class.
        /// </summary>
        /// <param name="services">Services configured from the ".bot" file.</param>
        public SysforeAIBotBot(IOptionsSnapshot<AppSettings> appSettings, IOptionsSnapshot<DialogFlow> dialogFlow, SysforeAIBotAccessors accessors)
        {
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
            _appSettings = appSettings.Value;
            _dialogFlow = dialogFlow.Value;
            _services = BotServiceHelper.InitBotServices(_appSettings);
        }

        

        /// <summary>
        /// Every conversation turn for our NLP Dispatch Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response, with no stateful conversation.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message && !turnContext.Responded)
            {
                // Get the intent recognition result
                var recognizerResult = await _services.LuisRecognizerService.RecognizeAsync(turnContext, cancellationToken);
                var topIntent = recognizerResult?.GetTopScoringIntent();

                if (topIntent == null)
                {
                    await turnContext.SendActivityAsync("Unable to get the top intent.");
                }
                else
                {
                    await DispatchToTopIntentAsync(turnContext, recognizerResult, topIntent, cancellationToken);
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                // Send a welcome message to the user and tell them what actions they may perform to use this bot
                if (turnContext.Activity.MembersAdded != null)
                {
                    await SendWelcomeMessageAsync(turnContext, cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected", cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// On a conversation update activity sent to the bot, the bot will
        /// send a message to the any new user(s) that were added.
        /// </summary>
        /// <param name="turnContext">Provides the <see cref="ITurnContext"/> for the turn of the bot.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>>A <see cref="Task"/> representing the operation result of the Turn operation.</returns>
        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id == turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync("Hey there!",
                        cancellationToken: cancellationToken);
                    await turnContext.SendActivityAsync(
                        $"I am your virtual assistant. {WelcomeText}",
                        cancellationToken: cancellationToken);
                }
            }
        }

        /// <summary>
        /// Depending on the intent from Dispatch, routes to the right LUIS model or QnA service.
        /// </summary>
        private async Task DispatchToTopIntentAsync(ITurnContext context, RecognizerResult recognizerResult, (string intent, double score)? topIntent, CancellationToken cancellationToken = default(CancellationToken))
        {
            //if (_services.QnAServices.ContainsKey(topIntent.Value.intent))
            //{
            //    await DispatchToQnAMakerAsync(context, topIntent.Value.intent);
            //}
            //else if (_services.Intents.Contains(topIntent.Value.intent))
            //{
            //    //Handle Lead generation
            //}
            //else
            //{
            //    await context.SendActivityAsync($"Dispatch intent: {topIntent.Value.intent} ({topIntent.Value.score}).");
            //}
            ConversationFlow flow = await _accessors.ConversationFlowAccessor.GetAsync(context, () => new ConversationFlow());
            bool isInDialogFlow = await _accessors.IsInDialogFlow.GetAsync(context, () => { return false; });
            if (flow.LastQuestionAsked != ConversationFlow.Question.None)
            {
                UserProfile profile = await _accessors.UserProfileAccessor.GetAsync(context, () => new UserProfile());

                await FillOutUserProfileAsync(flow, profile, context);

                // Update state and save changes.
                await _accessors.ConversationFlowAccessor.SetAsync(context, flow);
                await _accessors.ConversationState.SaveChangesAsync(context);

                await _accessors.UserProfileAccessor.SetAsync(context, profile);
                await _accessors.UserState.SaveChangesAsync(context);
            }
            else if(isInDialogFlow)
            {
                var node = await _accessors.Node.GetAsync(context, () => LoadDialogFlowConfig());
                foreach (var branch in node.Branches)
                {
                    if (context.Activity.Text == branch.Answer)
                    {
                        node = branch;
                        break;
                    }
                }
                if (node.Branches != null && node.Branches.Count() > 0){                   

                    var reply = context.Activity.CreateReply(node.Question);
                    reply.Type = ActivityTypes.Message;
                    reply.TextFormat = TextFormatTypes.Plain;

                    var actions = new List<CardAction>();

                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = node.Branches.Select(b => new CardAction()
                        {
                            Title = b.Answer,
                            Type = ActionTypes.ImBack,
                            Value = b.Answer
                        }).ToList()
                    };

                    await _accessors.Node.SetAsync(context, node);
                    await _accessors.ConversationState.SaveChangesAsync(context);

                    await context.SendActivityAsync(reply);
                }
                else
                {
                    var responseMessage = GetFinalResponse(node.Question);

                    await _accessors.Node.DeleteAsync(context);
                    await _accessors.IsInDialogFlow.DeleteAsync(context);
                    await _accessors.ConversationState.SaveChangesAsync(context);

                    await context.SendActivityAsync(responseMessage);
                }
            }
            else if (_services.Intents.Contains(topIntent.Value.intent) && topIntent.Value.score >= 0.75)
            {
                if (topIntent.Value.intent == "Greeting")
                {
                    await context.SendActivityAsync($"Hey!");
                }
                else if (topIntent.Value.intent == "LeadGeneration")
                {
                    UserProfile profile = await _accessors.UserProfileAccessor.GetAsync(context, () => new UserProfile());

                    await FillOutUserProfileAsync(flow, profile, context);

                    // Update state and save changes.
                    await _accessors.ConversationFlowAccessor.SetAsync(context, flow);
                    await _accessors.ConversationState.SaveChangesAsync(context);

                    await _accessors.UserProfileAccessor.SetAsync(context, profile);
                    await _accessors.UserState.SaveChangesAsync(context);
                }
                else if (topIntent.Value.intent == "EndOfConversation")
                {
                    var msg = @"Great! Hope your queries are resolved. Had a nice time talking to you.";
                    await context.SendActivityAsync(msg);
                }
                else
                {
                    var msg = @"Sorry. Didn't get that. Let's have another go!";
                    await context.SendActivityAsync(msg);
                }
            }
            else
            {
                await DispatchToQnAMakerAsync(context);
            }
        }

        /// <summary>
        /// Dispatches the turn to the request QnAMaker app.
        /// </summary>
        private async Task DispatchToQnAMakerAsync(ITurnContext context, string appName=null, CancellationToken cancellationToken = default(CancellationToken))
        {
            //if (!string.IsNullOrEmpty(context.Activity.Text))
            //{
            //    var results = await _services.QnAServices[appName].GetAnswersAsync(context);
            //    if (results.Any())
            //    {
            //        await context.SendActivityAsync(results.First().Answer, cancellationToken: cancellationToken);
            //    }
            //    else
            //    {
            //        await context.SendActivityAsync($"Couldn't find an answer in knowledge base.");
            //    }
            //}
            var response = !string.IsNullOrEmpty(appName)? await _services.QnAServices[appName].GetAnswersAsync(context): await _services.QnAServices.First().Value.GetAnswersAsync(context);
            if (response != null && response.Length > 0 && response[0].Score>=0.30)
            {
                await context.SendActivityAsync(response[0].Answer, cancellationToken: cancellationToken);
            }
            else
            {
                //var msg = @"Oops! Didn't get that. Let's have another go!";
                //await context.SendActivityAsync(msg, cancellationToken: cancellationToken);
                var node =  LoadDialogFlowConfig();
                var reply = context.Activity.CreateReply(node.Question);
                reply.Type = ActivityTypes.Message;
                reply.TextFormat = TextFormatTypes.Plain;

                var actions = new List<CardAction>();

                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = node.Branches.Select(b => new CardAction()
                    {
                        Title = b.Answer,
                        Type = ActionTypes.ImBack,
                        Value = b.Answer
                    }).ToList()
                };

                await _accessors.Node.SetAsync(context, node);
                await _accessors.IsInDialogFlow.SetAsync(context, true);
                await _accessors.ConversationState.SaveChangesAsync(context);

                await context.SendActivityAsync(reply);
            }
        }


        private static async Task FillOutUserProfileAsync(ConversationFlow flow, UserProfile profile, ITurnContext turnContext)
        {
            string input = turnContext.Activity.Text?.Trim();
            string message;
            switch (flow.LastQuestionAsked)
            {
                case ConversationFlow.Question.None:
                    await turnContext.SendActivityAsync("Thanks for the query. We'll reach out to you on the same. For contact purposes, may I know who this is?");
                    flow.LastQuestionAsked = ConversationFlow.Question.Name;
                    break;
                case ConversationFlow.Question.Name:
                    if (ValidateName(input, out string name, out message))
                    {
                        profile.Name = name;
                        await turnContext.SendActivityAsync($"Hi {profile.Name}.");
                        await turnContext.SendActivityAsync("Could you share your email address?");
                        flow.LastQuestionAsked = ConversationFlow.Question.EmailId;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }

                case ConversationFlow.Question.EmailId:
                    if (ValidateEmail(input, out string email, out message))
                    {
                        profile.EmailId = email;
                        await turnContext.SendActivityAsync("Finally, please share your phone number so that we can be in touch with you soon.");
                        flow.LastQuestionAsked = ConversationFlow.Question.Phone;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }

                case ConversationFlow.Question.Phone:
                    if (ValidatePhone(input, out string phone, out message))
                    {
                        profile.Phone = phone;
                        await turnContext.SendActivityAsync($"Thanks for providing the details {profile.Name}.");
                        await turnContext.SendActivityAsync($"We'll get back to you on your email: {profile.EmailId} or phone: {profile.Phone}.");
                        //await turnContext.SendActivityAsync(Activity.CreateEndOfConversationActivity());
                        //await turnContext.SendActivityAsync($"Type anything to run the bot again.");
                        flow.LastQuestionAsked = ConversationFlow.Question.None;
                        await UpdateGoogleSheets(profile);
                        profile = new UserProfile();
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
            }
        }

        private static async Task UpdateGoogleSheets(UserProfile userProfile)
        {
            try
            {
                UserCredential credential;

                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    //Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                // Define request parameters.
                String spreadsheetId = "164xPSbhI2UTPhiNMPh1WOgbrz6oge7BtNok5FQ1nbQQ";
                String range = "Sheet1";
                //SpreadsheetsResource.ValuesResource.GetRequest request =
                //        service.Spreadsheets.Values.Get(spreadsheetId, range);

                // Prints the names and majors of students in a sample spreadsheet:
                // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
                //ValueRange response = request.Execute();
                //IList<IList<Object>> values = new List<List<Object>> { new List<Object> {userProfile.Name } } //response.Values;
                //if (values != null && values.Count > 0)
                //{
                //    //Console.WriteLine("Name, Major");
                //    foreach (var row in values)
                //    {
                //        // Print columns A and E, which correspond to indices 0 and 4.
                //        //Console.WriteLine("{0}, {1}", row[0], row[4]);
                //    }
                //}
                //else
                //{
                //    //Console.WriteLine("No data found.");
                //}
                //Console.Read();
                IList<IList<Object>> values = new List<IList<Object>> { new List<Object> { userProfile.Name, userProfile.EmailId, userProfile.Phone } };

                ValueRange body = new ValueRange();
                body.Values = values;
                var cmd = service.Spreadsheets.Values.Append(body, spreadsheetId, range);
                cmd.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                var res = cmd.Execute();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static bool ValidateName(string input, out string name, out string message)
        {
            name = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter a name that contains at least one character.";
            }
            else
            {
                name = input.Trim();
            }

            return message is null;
        }

        private static bool ValidateEmail(string input, out string email, out string message)
        {

            email = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Oops! Please enter a valid email address.";
            }
            else
            {
                email = input.Trim();

                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(email);
                if (!match.Success)
                {
                    message = "Oops! Please enter a valid email address.";
                }
            }

            return message is null;
        }

        private static bool ValidatePhone(string input, out string phone, out string message)
        {
            phone = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Oops! Please enter a valid phone number.";
            }
            else
            {
                phone = input.Replace(" ", "");
                if (phone.Length < 10 || phone.Length > 15)
                {
                    message = "Oops! Please enter a valid phone number.";
                }
            }

            return message is null;
        }

        private DialogFlow LoadDialogFlowConfig()
        {
            return _dialogFlow;
        }

        private string GetFinalResponse(string question)
        {
            return $"Final answer for the question \"{question}\" .";
        }
    }
}
