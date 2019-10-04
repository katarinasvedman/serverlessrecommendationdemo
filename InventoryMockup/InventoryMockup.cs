using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Mockups
{
    public static class Inventory
    {
        [FunctionName("InventoryMockup")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Inventory function processed a request.");

            string item = req.Query["Item"];

            //Inventory is out of stock for items Women's Sandal and Men's Fleece Jacket 
            if( item == "Women's Sandal" || item == "Men's Fleece Jacket")
             return new NotFoundResult();

            var result = item != null
               ? (ActionResult)new OkObjectResult($"Item:, {item}")
               : new BadRequestObjectResult("Please pass an item on the query string or in the request body");

            return await Task.FromResult(result);
        }
    }
}
