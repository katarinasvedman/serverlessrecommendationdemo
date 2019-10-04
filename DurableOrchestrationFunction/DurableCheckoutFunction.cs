using System.Collections.Generic;
using System.Threading.Tasks;
using CartView;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DurableOrchestrationFunction
{
    public static class DurableCheckoutFunction
    {
        [FunctionName("DurableCheckoutFunction")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "changefeedlabdatabase",
            collectionName: "changefeeddemocollection",
            ConnectionStringSetting = "DBConnection",
            LeaseCollectionName = "leases_checkout",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            [OrchestrationClient] IDurableOrchestrationClient starter,
            ILogger log)
        {       
            // Iterate through modified documents from change feed.
            foreach (var doc in input)
            {
                var action = doc.GetPropertyValue<string>("Action");
                if (action != "Purchased")
                    return;

                string cartid = doc.GetPropertyValue<string>("CartID");
                log.LogInformation($"Cart to checkout: {cartid}");
                //Call durable function
                string instanceId = await starter.StartNewAsync("CheckoutFunction", new Item { Id = doc.GetPropertyValue<string>("Item") });

                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            }
        }
    }
}
