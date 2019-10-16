using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace CartView
{
    public static class CartViewProcessor
    {
        [FunctionName("CartViewProcessor")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "changefeedlabdatabase",
            collectionName: "changefeedlabcollection",
            ConnectionStringSetting = "DBconnection",
            LeaseCollectionName = "cartlease",
            CreateLeaseCollectionIfNotExists = true 
            //,StartFromBeginning = true //For demo of reading event store
            )]IReadOnlyList<Document> input,
            [DurableClient] IDurableEntityClient entityClient,
            ILogger log)
        {
            // Iterate through modified documents from change feed.
            foreach (var doc in input)
            {
                var action = doc.GetPropertyValue<string>("Action");
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
            var cartId = doc.GetPropertyValue<string>("CartID");

            return new EntityId("Cart", cartId);
        }

        private static Item getItem(Document doc)
        {
            // Get context information.
            var itemCategory = doc.GetPropertyValue<string>("Item");
            var itemPrice = doc.GetPropertyValue<double>("Price");
            var eTag = doc.ETag;

            return new Item { Id = itemCategory, Price = itemPrice, ETag = eTag };                        
        }
    }
}