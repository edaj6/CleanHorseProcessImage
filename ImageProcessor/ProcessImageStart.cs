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
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace ImageProcessor
{
    public static class ProcessImageStart
    {
        private static string accountName;
        private static string accountKey;

        [FunctionName("ProcessImageStart")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
                .AddEnvironmentVariables()
                .Build();

            accountName = config["StorageAccountName"];
            accountKey = config["StorageAccountKey"];

            var image = "";
            var tempImageLocation = "";

            try
            {
                var img = req.Form.Files[0];
                image = img.FileName;
                
                if (img == null)
                {
                    return new BadRequestErrorMessageResult("Please pass the image in the request body");
                }

                tempImageLocation = await SaveImageAsync(img);
            }
            catch (Exception ex)
            {

            }

            log.LogInformation($"About to start orchestration for {image}");
            var orchestrationId = await starter.StartNewAsync("OrcProcessImage", image);

            return starter.CreateCheckStatusResponse(req, orchestrationId);
        }

        private static async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            Uri baseUri = new Uri("https://storagejakob.blob.core.windows.net");            
            CloudBlobClient client = new CloudBlobClient(baseUri, new StorageCredentials(accountName, accountKey));

            try
            {
                StreamReader streamReader = new StreamReader(imageFile.OpenReadStream());
                var container = client.GetContainerReference("blob-images");
                var blob = container.GetBlockBlobReference(imageFile.FileName);
                await blob.UploadFromStreamAsync(streamReader.BaseStream);

                return new Uri(baseUri, $"/blob-images/{imageFile.FileName}").ToString();
            }
            catch (Exception exception)
            {
                throw;
            }

        }
    }
}
