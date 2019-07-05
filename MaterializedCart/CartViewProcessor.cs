using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace MaterializedCart
{
    public static class CartViewProcessor
    {
        [FunctionName("CartViewProcessor")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "changefeedlabdatabase",
            collectionName: "changefeedlabcollection",
            ConnectionStringSetting = "DBconnection",
            LeaseCollectionName = "viewlease",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            [OrchestrationClient] IDurableOrchestrationClient entityClient,
            ILogger log)
        {
            // Iterate through modified documents from change feed.
            foreach (var doc in input)
            {
                string action = doc.GetPropertyValue<string>("Action");
                log.LogInformation($"Cart action to process: {action}");
                switch (action)
                {
                    case "Purchased":
                        await entityClient.SignalEntityAsync(createEntityId(doc), "purchased", null);
                        break;
                    case "Added":
                        await entityClient.SignalEntityAsync(createEntityId(doc), "add", getItem(doc));
                        break;
                    case "Removed":
                        await entityClient.SignalEntityAsync(createEntityId(doc), "remove", getItem(doc));
                        break;
                }
            }
            
        }

        private static EntityId createEntityId(Document doc)
        {
            // The "Cart/{cartid}" entity is created on-demand.
            string cartid = doc.GetPropertyValue<string>("CartID");

            return new EntityId("Cart", cartid);
        }

        private static Item getItem(Document doc)
        {
            // Get context information.
            string itemCategory = doc.GetPropertyValue<string>("Item");
            double itemPrice = doc.GetPropertyValue<double>("Price");            

            return new Item { Id = itemCategory, Price = itemPrice };                        
        }
    }
}