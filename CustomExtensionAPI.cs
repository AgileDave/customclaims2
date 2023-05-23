using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Customer.StoreLocator
{
    public static class CustomExtensionAPI
    {
        [FunctionName("CustomExtensionAPI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Read the correlation ID from the Azure AD  request    
            string correlationId = data?.data.authenticationContext.correlationId;
            string ip = data?.data.authenticationContext.client.ip;

            // Claims to return to Azure AD
            ResponseContent r = new ResponseContent();
            r.data.actions[0].claims.correlationId = correlationId;
            r.data.actions[0].claims.clientIp = ip;
            r.data.actions[0].claims.apiVersion = "1.0.0";
            r.data.actions[0].claims.dateOfBirth = "01/01/2000";
            r.data.actions[0].claims.storeNumber = "123";
            r.data.actions[0].claims.customRoles.Add("Writer");
            r.data.actions[0].claims.customRoles.Add("Editor");
            return new OkObjectResult(r);
        }
    }

    public class ResponseContent
    {
        [JsonProperty("data")]
        public Data data { get; set; }
        public ResponseContent()
        {
            data = new Data();
        }
    }

    public class Data
    {
        [JsonProperty("@odata.type")]
        public string odatatype { get; set; }
        public List<Action> actions { get; set; }
        public Data()
        {
            odatatype = "microsoft.graph.onTokenIssuanceStartResponseData";
            actions = new List<Action>();
            actions.Add(new Action());
        }
    }

    public class Action
    {
        [JsonProperty("@odata.type")]
        public string odatatype { get; set; }
        public Claims claims { get; set; }
        public Action()
        {
            odatatype = "microsoft.graph.provideClaimsForToken";
            claims = new Claims();
        }
    }

    public class Claims
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string correlationId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string dateOfBirth { get; set; }
        public string apiVersion { get; set; }
        public string clientIp { get; set; }
        public string storeNumber { get; set; }
        public List<string> customRoles { get; set; }
        public Claims()
        {
            customRoles = new List<string>();
        }
    }
}
