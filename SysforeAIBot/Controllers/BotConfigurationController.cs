using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SysforeAIBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SysforeAIBot.Extensions;
using SysforeAIBot.DTO;
using Newtonsoft.Json;

namespace SysforeAIBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotConfigurationController : ControllerBase
    {
        private readonly IWritableOptions<AppSettings> _writeOptions;
        public AppSettings _appSettings { get; set; }

        public BotConfigurationController(
            IWritableOptions<AppSettings> writeOptions,
            IOptionsSnapshot<AppSettings> appSettings
            )
        {
            _writeOptions = writeOptions;
            _appSettings = appSettings.Value;
        }
        /// <summary>
        /// Update Luis app reference detils for bot's external access.
        /// </summary>
        /// <param name="luisAppInput"></param>
        /// <returns></returns>
        [HttpPut("UpdateLuisApp")]
        [ProducesResponseType(400)]
        public IActionResult UpdateLuisApp([FromBody]LuisAppInput luisAppInput)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _writeOptions.Update(opt =>
            {
                opt.luisApp = new LuisApp() {
                    type = luisAppInput.type,
                    appId = luisAppInput.appId,
                    authoringKey = luisAppInput.authoringKey,
                    id = luisAppInput.id,
                    name = luisAppInput.name,
                    region = luisAppInput.region,
                    serviceIds = luisAppInput.serviceIds,
                    subscriptionKey = luisAppInput.subscriptionKey,
                    version = luisAppInput.version
                };
                //opt.luisIntents = new List<string>() { "l_SampleQnA", "l_TestQnA" };
            });
            return NoContent();
        }

        /// <summary>
        /// Updates the set of QnA services used by bot.
        /// </summary>
        /// <param name="qnAServicesInput"></param>
        /// <returns></returns>
        [HttpPut("UpdateQnAServices")]
        [ProducesResponseType(400)]
        public IActionResult UpdateQnAServices([FromBody]QnAServicesInput qnAServicesInput)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            List<QnaService> qnaServicesList = new List<QnaService>();
            foreach(var item in qnAServicesInput.qnaServices)
            {
                QnaService qnaService = new QnaService()
                {
                    type = item.type,
                    endpointKey = item.endpointKey,
                    hostname = item.hostname,
                    id = item.id,
                    kbId = item.kbId,
                    name = item.name,
                    subscriptionKey = item.subscriptionKey
                };
                qnaServicesList.Add(qnaService);
            }
            _writeOptions.Update(opt => {
                opt.qnaServices = qnaServicesList;
            });
            return NoContent();
        }

        /// <summary>
        /// Updates the set of Luis intents which are referred by bot.
        /// </summary>
        /// <param name="luisIntentsInput"></param>
        /// <returns></returns>
        [HttpPut("UpdateLuisIntents")]
        [ProducesResponseType(400)]
        public IActionResult UpdateLuisIntents(LuisIntentsInput luisIntentsInput)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            _writeOptions.Update(opt => {
                opt.luisIntents = luisIntentsInput.LuisIntents;
            });
            return NoContent();
        }

        /// <summary>
        /// Get the bot external application reference detasils.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetBotConfigDetails()
        {
            return new JsonResult(_appSettings);
        }
    }
}
