using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaterializedCart;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableOrchestrationFunction
{
    public static class DurableCheckoutFunction
    {
        [FunctionName("DurableCheckoutFunction")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "changefeedlabdatabase",
            collectionName: "changefeedlabcollection",
            ConnectionStringSetting = "DBConnection",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> input,
            [OrchestrationClient] IDurableOrchestrationClient starter,
            ILogger log)
        {       
            // Iterate through modified documents from change feed.
            foreach (var doc in input)
            {
                string action = doc.GetPropertyValue<string>("Action");
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
