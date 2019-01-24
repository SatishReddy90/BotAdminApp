using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdminBot.Models;
using Microsoft.AspNetCore.Http;
using AdminBot.Helpers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Web;

namespace AdminBot.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppSettings _appSettings;
        public HomeController(IOptionsMonitor<AppSettings> appSettingsAccessor)
        {
            _appSettings = appSettingsAccessor.CurrentValue;
        }
        public async Task<IActionResult> Index()
        {
            ViewData["QnADeleteUrl"] = _appSettings.QnAMaker.Host + _appSettings.QnAMaker.Service + _appSettings.QnAMaker.Methods.Delete;
            ViewData["QnASubScriptionKey"] = _appSettings.QnAMaker.SubscriptionKey;
            ViewData["QnAEndpointkeyUrl"] = _appSettings.QnAMaker.Host + _appSettings.QnAMaker.Service + _appSettings.QnAMaker.Methods.EndPointKeys;
            ViewData["QnAGetUrl"] = _appSettings.QnAMaker.Host + _appSettings.QnAMaker.Service + _appSettings.QnAMaker.Methods.Get;
            try
            {
                KBHelper kBHelper = new KBHelper(_appSettings);
                RootObject rootObject = await kBHelper.GetKBsByUser();
                return View(rootObject.knowledgebases);
            }
            catch(Exception ex)
            {
                return View(new List<Knowledgebase>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult Create()
        {
            ViewData["QnACreateUrl"] = _appSettings.QnAMaker.Host + _appSettings.QnAMaker.Service + _appSettings.QnAMaker.Methods.Create;
            ViewData["QnAHost"] = _appSettings.QnAMaker.Host;
            ViewData["QnAService"] = _appSettings.QnAMaker.Service;
            ViewData["QnASubScriptionKey"] = _appSettings.QnAMaker.SubscriptionKey;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateKBModel createKBModel)
        {
            try
            {
                CreateKBRequest createKBRequest = new CreateKBRequest();
                createKBRequest.name = createKBModel.Name;
                createKBRequest.qnaList = new List<QnaList>() {
                    new QnaList(){
                        id = 0,
                        answer = "Use this admin app to create the knowledge base",
                        source = "Admin App",
                        questions = new List<string>(){ "What admin app can do?" },
                        metadata = new List<Metadata>()
                        {
                            new Metadata()
                            {
                                name = "category",
                                value = "api"
                            }
                        }
                    }
                };
                createKBRequest.urls = Uri.UnescapeDataString(createKBModel.Urls).Split(',').ToList<string>();
                //createKBRequest.urls = ExtractUrls(createKBModel.Urls, false);
                createKBRequest.files = new List<object>();
                string kb = JsonConvert.SerializeObject(createKBRequest);
                KBHelper kBHelper = new KBHelper(_appSettings);
                var headerValues = kBHelper.CreateKB(JsonConvert.SerializeObject(createKBRequest));
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            KBHelper kBHelper = new KBHelper(_appSettings);
            Knowledgebase knowledgebase = await kBHelper.GetKB(id);
            return View(knowledgebase);
        }

        public List<string> ExtractUrls(string websiteUrl, bool isRecursive)
        {
            UrlExtractor urlExtractor = new UrlExtractor();
            var list = urlExtractor.GetUrls(websiteUrl, isRecursive);
            return list.ToList();
            //UrlExtractHelper urlExtractHelper = new UrlExtractHelper();
            //return urlExtractHelper.RetrieveUrls(websiteUrl);

        }

        public async Task<bool> Delete(string kbid)
        {
            KBHelper kBHelper = new KBHelper(_appSettings);
            return await kBHelper.DeleteKB(kbid);

        }
    }
}
