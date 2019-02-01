using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminBot.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AdminBot.Controllers
{
    public class DialogFlowController : Controller
    {
        private readonly AppSettings _appSettings;
        public DialogFlowController(IOptionsMonitor<AppSettings> appSettingsAccessor)
        {
            _appSettings = appSettingsAccessor.CurrentValue;
        }
        // GET: /<controller>/
        public IActionResult Index()
        {
            ViewData["BotBaseUrl"] = _appSettings.BotApi.BaseUrl;
            return View();
        }
    }
}
