using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessor
{

    public static class ProcessImageActivities
    {
        [FunctionName("ActivityValidate")]
        public static async Task<string> ValidateImage([ActivityTrigger] string input, ILogger log)
        {
            log.LogInformation($"Validating {input}");
            
            // simulate doing the activity
            await Task.Delay(5000);
            return "validimagelocation.jpg";
        }

        [FunctionName("ActivityResize")]
        public static async Task<string> ResizeImage([ActivityTrigger] string input, ILogger log)
        {
            log.LogInformation($"ResizeImage {input}");

            // simulate doing the activity
            await Task.Delay(5000);
            return "validimagelocation.jpg";
        }

        [FunctionName("ActivityCreateThumbnail")]
        public static async Task<string> CreateThumbnail([ActivityTrigger] string input, ILogger log)
        {
            log.LogInformation($"CreateThumbnail {input}");

            // simulate doing the activity
            await Task.Delay(5000);
            return "validimagelocation.jpg";
        }

        [FunctionName("ActivitySendMail")]
        public static async Task<bool> SendMail([ActivityTrigger] string input, ILogger log)
        {
            log.LogInformation($"SendMail {input}");

            // simulate doing the activity
            await Task.Delay(5000);
            return true;
        }
    }
}
