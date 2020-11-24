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
using Microsoft.Azure.WebJobs.Host;

namespace ImageProcessor
{
    public static class ProcesImageOrchestra
    {
        [FunctionName("OrcProcessImage")]
        public static async Task<object> ProcessImage(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            var imageLocation = ctx.GetInput<string>();

            if (!ctx.IsReplaying)
                log.LogInformation($"About to call validate image content for {imageLocation}");

            var validImageLocation = await
                ctx.CallActivityAsync<string>("ActivityValidate", imageLocation);
            
            if (validImageLocation == null)
            {
                log.LogWarning("Image failed validation");
                return new
                {
                    Status = "Error"
                };
            }

            if (!ctx.IsReplaying)
                log.LogInformation("About to call resize image");

            var resizedImageLocation = await
                ctx.CallActivityAsync<string>("ActivityResize", validImageLocation);

            if (!ctx.IsReplaying)
                log.LogInformation("About to call create thumbnail");

            var thumbnailLocation = await
                ctx.CallActivityAsync<string>("ActivityCreateThumbnail", resizedImageLocation);

            if (!ctx.IsReplaying)
                log.LogInformation("About to call send mail");

            var isMailSend = await
                ctx.CallActivityAsync<bool>("ActivitySendMail", resizedImageLocation);

            return new
            {
                Status = "Succes",
                Transcoded = resizedImageLocation,
                Thumbnail = thumbnailLocation,
                MailSend = isMailSend
            };

        }
    }
}
