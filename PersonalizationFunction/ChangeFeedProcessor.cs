//-----------------------------------------------------------------------
// <copyright file="ChangeFeedProcessor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved. 
// </copyright>
// <author>Serena Davis</author>
//-----------------------------------------------------------------------

/// <summary>
/// Azure Function triggered by Cosmos DB Change Feed that sends modified records to Event Hub
/// </summary>
namespace ChangeFeedFunction
{
    using System.Collections.Generic;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.CognitiveServices.Personalizer;
    using Microsoft.Azure.CognitiveServices.Personalizer.Models;
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// Processes events using Cosmos DB Change Feed.
    /// </summary>
    public static class ChangeFeedProcessor
    {      
        /// <summary>
        /// Processes modified records from Cosmos DB to generate recommendations.
        /// </summary>
        /// <param name="documents"> Modified records from Cosmos DB collections. </param>
        /// <param name="log"> Outputs modified records to Event Hub. </param>
        [FunctionName("ChangeFeedProcessor")]
        public static void Run(
            //change database name below if different than specified in the lab
            [CosmosDBTrigger(databaseName: "changefeedlabdatabase",
            //change the collection name below if different than specified in the lab
            collectionName: "changefeedlabcollection",
            ConnectionStringSetting = "DBconnection",
            LeaseConnectionStringSetting = "DBconnection",
            LeaseCollectionName = "leases2",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> documents, ILogger log)
        {
            // Get the actions list to choose from personalizer with their features.
            IList<RankableAction> actions = GetActions();

            string apikey = "2a16c124f3c1425db69659f32aaccc40";
            string endpoint = "https://westeurope.api.cognitive.microsoft.com/";
            PersonalizerClient client = InitializePersonalizerClient(endpoint, apikey);

            // Iterate through modified documents from change feed.
            foreach (var doc in documents)
            {
                if (doc.GetPropertyValue<string>("Action") == "Viewed")
                {
                    // Get context information.
                    string itemCategoryFeature = doc.GetPropertyValue<string>("Item");
                    if (itemCategoryFeature.StartsWith("W"))
                        itemCategoryFeature = "Women";
                    else
                        itemCategoryFeature = "Men";
                    string itemPriceFeature = doc.GetPropertyValue<string>("Price");

                    // Create current context from user specified data.
                    IList<object> currentContext = new List<object>() {
                    new { category = itemCategoryFeature },
                    new { price = itemPriceFeature }};

                    // Exclude an action for personalizer ranking. This action will be held at its current position.
                    IList<string> excludeActions = new List<string> { "juice" };

                    // Generate an ID to associate with the request.
                    string eventId = Guid.NewGuid().ToString();

                    // Rank the actions
                    var request = new RankRequest(actions, currentContext, excludeActions, eventId);
                    RankResponse response = client.Rank(request);

                    Console.WriteLine("\nPersonalizer service ranked the actions with the probabilities as below:");
                    foreach (var rankedResponse in response.Ranking)
                    {
                        log.LogInformation($"Id: {rankedResponse.Id}, Probability: {rankedResponse.Probability}");
                    }

                    // Send the reward for the action based on user response.
                    //client.Reward(response.EventId, new RewardRequest(reward));

                }



                /*
                 * "CartID": 4673,
        "Action": "Purchased",
        "Item": "Women's Flannel Shirt",
        "Price": 19.99,
                 * */
            }
        }

        private static IList<RankableAction> GetActions()
        {
            IList<RankableAction> actions = new List<RankableAction>
            {
                new RankableAction
                {
                    Id = "Women's Blazer Jacket",
                    Features =
                    new List<object>() { new { type = "Jacket", category = "Women" }, new { placement = "top" }, new { occation = "business" }, new { price = 87.5 } }
                },
                new RankableAction
                {
                    Id = "Women's Puffy Jacket",
                    Features =
                    new List<object>() { new { type = "Jacket", category = "Women" }, new { placement = "top" }, new { occation = "casual" }, new { price = 95.99 } }
                },
                new RankableAction
                {
                    Id = "Men's Fleece Jacket",
                    Features =
                    new List<object>() { new { type = "Jacket", category = "Men" }, new { placement = "top" }, new { occation = "casual" }, new { price = 65 } }
                },

                new RankableAction
                {
                    Id = "Men's Windbreaker Jacket",
                    Features =
                    new List<object>() { new { type = "Jacket", category = "Men" }, new { placement = "top" }, new { occation = "outdoor" }, new { price = 49.99 } }
                }
            };

            return actions;
        }

        /// <summary>
        /// Initializes the personalizer client.
        /// </summary>
        /// <param name="url">Azure endpoint</param>
        /// <returns>Personalizer client instance</returns>
        static PersonalizerClient InitializePersonalizerClient(string url, string ApiKey)
        {
            PersonalizerClient client = new PersonalizerClient(
                new ApiKeyServiceClientCredentials(ApiKey))
            { Endpoint = url };

            return client;
        }
    }
}
