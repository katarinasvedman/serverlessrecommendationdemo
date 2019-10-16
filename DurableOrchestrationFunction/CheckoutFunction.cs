using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CartView;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DurableOrchestrationFunction
{
    public static class CheckoutFunction
    {
        [FunctionName("CheckoutFunction")]
        public static async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var options = new RetryOptions(
                firstRetryInterval: System.TimeSpan.FromMinutes(1),
                maxNumberOfAttempts: 5);
            options.BackoffCoefficient = 2.0;

            var parallelTasks = new List<Task<bool>>
            {
                context.CallActivityWithRetryAsync<bool>("CallInventoryFunction", options, context.GetInput<Item>().Id),
                context.CallActivityWithRetryAsync<bool>("CallPaymentFunction", options, null)
            };

            var value = await Task.WhenAll(parallelTasks);

            /*
             * For demo purpose the tasks return 0 if they are successful. This means a sum > 0 will NOT be a good result.
             * If Inventory is != 0 then the item was not available and the customer should receive an email describing "out of stock " scenario.
             * If Inventory is ok and Payment had a problem then the item should be returned to inventory with a compensating transaction and the user should be notified.
             */
            return value.All(x => x == true);
            
        }

        [FunctionName("CallInventoryFunction")]
        public static async Task<bool> PurchaseItem([ActivityTrigger] string itemId, ILogger log)
        {
            log.LogInformation($"check inventory for item: {itemId}.");
            var client = new HttpClient();
            var values = new Dictionary<string, string>{{ "Item", itemId}};
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("http://localhost:5870/api/InventoryMockup?Item=" + itemId, content);

            return response.IsSuccessStatusCode;
        }

        [FunctionName("CallPaymentFunction")]
        public async static Task<bool> PayForItem([ActivityTrigger] string itemId, ILogger log)
        {
            HttpClient client = new HttpClient();
            var response = await client.PostAsync("http://localhost:5870/api/PaymentMockup?Item=" + itemId, null);

            return response.IsSuccessStatusCode;
        }
    }
}