using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Web.Http;

namespace ImageProcessor
{
    public static class ProcessImageStart
    {
        [FunctionName("ProcessImageStart")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string image = req.Query["image"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            dynamic data = JsonConvert.DeserializeObject(requestBody);

            image ??= data?.image;

            if (image == null)
            {
                return new BadRequestErrorMessageResult("Please pass the image location in the query string or in the request body");
            }

            log.LogInformation($"About to start orchestration for {image}");
            var orchestrationId = await starter.StartNewAsync("OrcProcessImage", image);

            return starter.CreateCheckStatusResponse(req, orchestrationId); 
        }
    }
}
