using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ChangeFeedPowerBIFunction
{
    public static class ChangeFeedPowerBIFunction
    {
        [FunctionName("ChangeFeedPowerBIFunction")]
        
        public static async void Run([CosmosDBTrigger(
            databaseName: "changefeedlabdatabase",
            collectionName: "changefeedlabcollection",
            ConnectionStringSetting = "DBconnection",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> documents,
            [EventHub("event-hub1", Connection = "EventHubNamespaceConnection")]IAsyncCollector<string> outputEvents,
            ILogger log)
        {  
            // Iterate through modified documents from change feed.
            foreach (var doc in documents)
            {              
                // Use Event Hub client to send the change events to event hub.
                await outputEvents.AddAsync(JsonConvert.SerializeObject(doc));
            }
        }
    }
}
