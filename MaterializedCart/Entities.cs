using System;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;
using System.Collections.Generic;

namespace MaterializedCart
{
    public class Cart
    {
        public string CartId;
        public double TotalAmount;
        public string TimeStamp;
        public List<Item> Items;

        public Cart(){}

        public Cart(string id)
        {
            this.CartId = id;
        }

        public static Cart FromDocument(Document document)
        {
            var result = new Cart()
            {
                CartId = document.GetPropertyValue<string>("deviceId"),
                TotalAmount = document.GetPropertyValue<double>("Price"),
                TimeStamp = document.GetPropertyValue<DateTime>("timestamp").ToString("yyyy-MM-ddTHH:mm:ssK")
            };
            return result;
        }

        public static Cart AddItem(string id, Document document)
        {
            double itemPrice = document.GetPropertyValue<double>("Price");
            // Read event store for cart and create object - REMOVE below
            Cart cart = new Cart()
            {
                CartId = id,
                TimeStamp = document.GetPropertyValue<DateTime>("timestamp").ToString("yyyy-MM-ddTHH:mm:ssK")
            };

            cart.TotalAmount += itemPrice;
            cart.Items.Add(new Item { Id = document.GetPropertyValue<string>("deviceId"), Price = document.GetPropertyValue<double>("Price") });            

            return cart;
        }
    }

    public class Item
    {
        public string Id { get; set; }
        public double Price { get; set; }
    }

    public class CartMaterializedView
    {
        [JsonProperty("id")]
        public string Name;

        [JsonProperty("totalAmount")]
        public double TotalAmount;

        [JsonProperty("lastValue")]
        public double LastValue;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("deviceId")]
        public string DeviceId;

        [JsonProperty("lastUpdate")]
        public string TimeStamp;
    }
}
