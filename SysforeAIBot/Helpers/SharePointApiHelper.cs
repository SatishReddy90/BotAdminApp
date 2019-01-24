using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Web;
using SysforeAIBot.DTO;
using SysforeAIBot.Models;

namespace SysforeAIBot.Helpers
{
    public class SharePointApiHelper
    {
        public async Task<List<Employee>> GetEmployeeDetails(EmployeeInput employeeInput)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            //Request Headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "f326a741a3b14320b7565f9289cbd8ff");
            var uri = "https://apim-sample-mgmtservice.azure-api.net/Sysfore/api/Values/EmployeeResults?" + queryString;
            HttpResponseMessage response;

            var myContent = JsonConvert.SerializeObject(employeeInput);
            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            response = await client.PostAsync(uri, byteContent);
            var body = response.Content;
            var jsonstr = await body.ReadAsStringAsync();
            List<Employee> employees = JsonConvert.DeserializeObject<List<Employee>>(jsonstr);
            return employees;
        }

    }
}
