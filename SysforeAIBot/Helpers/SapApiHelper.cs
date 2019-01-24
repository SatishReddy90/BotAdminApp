using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;
using SysforeAIBot.Models;

namespace SysforeAIBot.Helpers
{
    public class SapApiHelper
    {
        public async Task<List<Vehicle>> GetVehicleDetails(string model)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "f326a741a3b14320b7565f9289cbd8ff");
            var uri = "https://apim-sample-mgmtservice.azure-api.net/Sysfore/api/Values/MotorResults?" + queryString;
            HttpResponseMessage response;

            var input = new { Model = model };
            var myContent = JsonConvert.SerializeObject(input);
            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            response = await client.PostAsync(uri, byteContent);
            var body = response.Content;
            var jsonstr = await body.ReadAsStringAsync();
            List<Vehicle> vehicleList = JsonConvert.DeserializeObject<List<Vehicle>>(jsonstr);
            return vehicleList;
        }
    }
}
