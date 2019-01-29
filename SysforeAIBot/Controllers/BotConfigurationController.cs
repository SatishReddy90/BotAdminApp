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

namespace SysforeAIBot.Controllers
{
    [Route("api/[controller]")]
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

        [HttpPut]
        public void Update()
        {
            _writeOptions.Update(opt => {
                //opt.luisIntents = new List<string>() { "l_Lead_Generation","l_Sap", "l_SharePoint" };
                opt.luisIntents = new List<string>() { "l_SampleQnA", "l_TestQnA" };
            });
        }

        [HttpGet]
        public List<string> GetOptions()
        {
            return _appSettings.luisIntents;
        }
    }
}
