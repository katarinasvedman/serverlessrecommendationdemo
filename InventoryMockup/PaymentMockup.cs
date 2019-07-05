using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;

namespace Mockups
{
    public static class PaymentMockup
    {
        [FunctionName("PaymentMockup")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("PaymentMockup function processing a request. Going to sleep for a while...");
            Thread.Sleep(10000);

            //Random payment declined result
            Random rnd = new Random();
            int ok = rnd.Next(0, 100) % 3;
            return ok == 1
                ? (ActionResult)new OkObjectResult($"Payment ok")
                : new BadRequestObjectResult("Payment rejected");
        }
    }
}
