using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AdminBot.Models;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http.Headers;
using System.Threading;

namespace AdminBot.Helpers
{
    public class KBHelper
    {
        private readonly AppSettings _appSettings;
        public KBHelper(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        #region Get
        async Task<string> Get(string uri)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", _appSettings.QnAMaker.SubscriptionKey);

                var response = await client.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<KnowledgebaseResponse> GetKBsByUser()
        {
            var uri = _appSettings.QnAMaker.Host + _appSettings.QnAMaker.Service + _appSettings.QnAMaker.Methods.Get;
            var response = await Get(uri);
            var botResponse = await GetKbsFromBot();
            var botConfigDetails = JsonConvert.DeserializeObject<BotConfigDetails>(botResponse);
            var kbResponse = JObject.Parse(response).ToObject<KnowledgebaseResponse>();
            if (botConfigDetails != null && botConfigDetails.qnaServices != null)
            {
                foreach (var item in kbResponse.knowledgebases)
                {
                    item.IsEnabled = botConfigDetails.qnaServices.Exists(qna => qna.kbId == item.Id);
                }
            }
            return kbResponse;
        }

        public async Task<Knowledgebase> GetKB(string kb)
        {
            var uri = _appSettings.QnAMaker.Host + _appSettings.QnAMaker.Service + _appSettings.QnAMaker.Methods.Get + "/" + kb;
            var response = await Get(uri);
            return JsonConvert.DeserializeObject<Knowledgebase>(response);
        }

        public async Task<string> GetKbsFromBot()
        {
            string uri = "http://localhost:9632/api/BotConfiguration";
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(uri);
                var response = await client.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
        }
        #endregion

        #region Create
        private async Task<Response> Post(string uri, string body)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _appSettings.QnAMaker.SubscriptionKey);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                return new Response(response.Headers, responseBody);
            }
        }

        async Task<Response> PostCreateKB(string kb)
        {
            string uri = _appSettings.QnAMaker.Host + _appSettings.QnAMaker.Service + _appSettings.QnAMaker.Methods.Create;
            return await Post(uri, kb);
        }

        private async Task<Response> GetStatus(string operation)
        {
            string uri = _appSettings.QnAMaker.Host + _appSettings.QnAMaker.Service + operation;
            
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", _appSettings.QnAMaker.SubscriptionKey);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                return new Response(response.Headers, responseBody);
            }
        }

        public async Task<List<string>> CreateKB(string kb)
        {

            var response = await PostCreateKB(kb);
            var operation = response.headers.GetValues("Location").First();
            List<string> headerValues = new List<string>();
            var done = false;
            while (true != done)
            {
                response = await GetStatus(operation);

                var fields = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.response);

                String state = fields["operationState"];
                if (state.CompareTo("Running") == 0 || state.CompareTo("NotStarted") == 0)
                {
                    var wait = response.headers.GetValues("Retry-After").First();
                    headerValues.Add(wait);
                    Thread.Sleep(Int32.Parse(wait) * 1000);
                }
                else
                {
                    done = true;
                }
            }
            return headerValues;
        }
        #endregion

        #region Update

        #endregion

        #region Delete

        async Task<bool> Delete(string uri)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Delete;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", _appSettings.QnAMaker.SubscriptionKey);

                var response = await client.SendAsync(request);

                return (response.IsSuccessStatusCode);
            }
        }

        public async Task<bool> DeleteKB(string kbId)
        {
            var uri = _appSettings.QnAMaker.Host + _appSettings.QnAMaker.Service + _appSettings.QnAMaker.Methods.Delete + kbId;
            return await Delete(uri);
        }

        #endregion
    }

    public struct Response
    {
        public HttpResponseHeaders headers;
        public string response;

        public Response(HttpResponseHeaders headers, string response)
        {
            this.headers = headers;
            this.response = response;
        }
    }
}
