using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MaterializedCart;

namespace CartView
{
    public static class CartView
    {
        [FunctionName("CartView")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [OrchestrationClient] IDurableOrchestrationClient entityClient,
            ILogger log)
        {
            //Get cartid from http request
            string cartid = req.Query["CartId"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            cartid = cartid ?? data?.CartId;
            if(cartid == null)
                return new BadRequestObjectResult("Please pass a cartid in the query string or in the request body (CartId:xxx)");

            //Read state of Cart Entity. State will give a Cart with a list of Items and a total amount
            EntityStateResponse<Cart> entityresult = await entityClient.ReadEntityStateAsync<Cart>(new EntityId("Cart", cartid));
            if (entityresult.EntityExists)
            {
                //Create cart JSON
                Cart cart = entityresult.EntityState;
                dynamic cartAsJson = JsonConvert.SerializeObject(cart);
                return (ActionResult)new OkObjectResult(cartAsJson);
            }

            return new BadRequestObjectResult($"Someting went wrong reading cart: {cartid}");
        }
    }
}
