using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Swashbuckle.AspNetCore.Swagger;
using SysforeAIBot.Models;
using SysforeAIBot.NlpDispatch;
using System.IO;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;

namespace SysforeAIBot.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureWritable<T>(
            this IServiceCollection services,
            IConfigurationSection section,
            string file = "appsettings.json") where T : class, new()
        {
            services.Configure<T>(section);
            services.AddTransient<IWritableOptions<T>>(provider =>
            {
                var environment = provider.GetService<IHostingEnvironment>();
                var options = provider.GetService<IOptionsMonitor<T>>();
                return new WritableOptions<T>(environment, options, section.Key, file);
            });
        }

        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Sysfore Bot API",
                    Version = "v1",
                    Description = "Use this Web API to configure the bot files",
                    TermsOfService = "None"
                });
            });
        }

        public static void ConfigureAppSettings(this IServiceCollection services, IConfiguration Configuration)
        {
            
            Action onChange = () =>
            {
                var appSettingsDescriptorToRemove = services.Where(d => d.ServiceType.ToString().Contains("AppSettings")).ToList();
                foreach (var item in appSettingsDescriptorToRemove)
                {
                    services.Remove(item);
                }
                services.ConfigureWritable<AppSettings>(Configuration.GetSection("AppSettings"));

                var dialogFlowDescriptorToRemove = services.Where(d => d.ServiceType.ToString().Contains("DialogFlow")).ToList();
                foreach (var item in dialogFlowDescriptorToRemove)
                {
                    services.Remove(item);
                }
                services.ConfigureWritable<DialogFlow>(Configuration.GetSection("DialogFlow"));
            };

            ChangeToken.OnChange(() => Configuration.GetReloadToken(), onChange);
        }
        
        public static void ConfigureConversationStateAccessors(this IServiceCollection services)
        {
            // Create and register state accessors.
            // Accessors created here are passed into the IBot-derived class on every turn.
            services.AddSingleton<SysforeAIBotAccessors>(sp =>
            {
                //var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                //if (options == null)
                //{
                //    throw new InvalidOperationException("BotFrameworkOptions must be configured prior to setting up the state accessors");
                //}

                //var conversationState = options.State.OfType<ConversationState>().FirstOrDefault();
                //if (conversationState == null)
                //{
                //    throw new InvalidOperationException("ConversationState must be defined and added before adding conversation-scoped state accessors.");
                //}

                IStorage storage = new MemoryStorage();
                ConversationState conversationState = new ConversationState(storage);
                UserState userState = new UserState(storage);
                // Create the custom state accessor.
                // State accessors enable other components to read and write individual properties of state.
                var accessors = new SysforeAIBotAccessors(conversationState, userState)
                {
                    ConversationFlowAccessor = conversationState.CreateProperty<ConversationFlow>(SysforeAIBotAccessors.ConversationFlowName),
                    UserProfileAccessor = userState.CreateProperty<UserProfile>(SysforeAIBotAccessors.UserProfileName),
                    Node = conversationState.CreateProperty<DialogFlow>(SysforeAIBotAccessors.NodeName),
                    IsInDialogFlow = conversationState.CreateProperty<bool>(SysforeAIBotAccessors.IsInDialogFlowName)
                };

                return accessors;
            });
        }

        public static void ConfigureBot(this IServiceCollection services, IConfiguration Configuration, ILoggerFactory _loggerFactory, bool _isProduction)
        {
            
            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;
            if (!File.Exists(botFilePath))
            {
                throw new FileNotFoundException($"The .bot configuration file was not found. botFilePath: {botFilePath}");
            }

            // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
            var botConfig = BotConfiguration.Load(botFilePath ?? @".\SysforeAIBot.bot", secretKey);
            services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot configuration file could not be loaded. botFilePath: {botFilePath}"));

            // Retrieve current endpoint.
            var environment = _isProduction ? "production" : "development";
            var service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment);
            if (!(service is EndpointService endpointService))
            {
                throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");
            }
            services.AddBot<SysforeAIBotBot>(options =>
            {

                options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

                // Creates a logger for the application to use.
                ILogger logger = _loggerFactory.CreateLogger<SysforeAIBotBot>();

                // Catches any errors that occur during a conversation turn and logs them.
                options.OnTurnError = async (context, exception) =>
                {
                    logger.LogError($"Exception caught : {exception}");
                    await context.SendActivityAsync("Sorry, it looks like something went wrong.");
                };

                // The Memory Storage used here is for local bot debugging only. When the bot
                // is restarted, everything stored in memory will be gone.
                IStorage dataStore = new MemoryStorage();

                // For production bots use the Azure Blob or
                // Azure CosmosDB storage providers. For the Azure
                // based storage providers, add the Microsoft.Bot.Builder.Azure
                // Nuget package to your solution. That package is found at:
                // https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/
                // Uncomment the following lines to use Azure Blob Storage
                // //Storage configuration name or ID from the .bot file.
                // const string StorageConfigurationId = "<STORAGE-NAME-OR-ID-FROM-BOT-FILE>";
                // var blobConfig = botConfig.FindServiceByNameOrId(StorageConfigurationId);
                // if (!(blobConfig is BlobStorageService blobStorageConfig))
                // {
                //    throw new InvalidOperationException($"The .bot file does not contain an blob storage with name '{StorageConfigurationId}'.");
                // }
                // // Default container name.
                // const string DefaultBotContainer = "<DEFAULT-CONTAINER>";
                // var storageContainer = string.IsNullOrWhiteSpace(blobStorageConfig.Container) ? DefaultBotContainer : blobStorageConfig.Container;
                // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage(blobStorageConfig.ConnectionString, storageContainer);

                // Create Conversation State object.
                // The Conversation State object is where we persist anything at the conversation-scope.
                var conversationState = new ConversationState(dataStore);

                options.State.Add(conversationState);
            });

        }
    }
}
