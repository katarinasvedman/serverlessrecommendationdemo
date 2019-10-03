using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace MaterializedCart
{
    public static class CartDurableEntity
    {
        [FunctionName("Cart")]
        public static void Cart(
            [EntityTrigger(EntityName = "Cart")] IDurableEntityContext ctx,
            ILogger log)
        {
            var userAction = ctx.OperationName;
            //Do not care about Viewed event
            if (userAction == "viewed")
            {
                return;
            }
            //Checked out - Cart is no longer needed 
            if (userAction == "purchased")
            {
                log.LogInformation($"Cart is destructed: {ctx.Self}");
                ctx.DestructOnExit();
                return;
            }

            //If Cart doesn't exist it will initialize with an empty item list
            var cart = ctx.GetState<Cart>( () =>  new Cart( ctx.Key ));

            //Read item from "request" input
            var item = ctx.GetInput<Item>();
            if (item == null)
                ctx.Return(new Exception($"No Item found in request to Cart {ctx.Self}"));

            //Duplicate detection. Don't add duplicates.
            if(CheckForDuplicate(cart, item.ETag))
                return;

            log.LogInformation($"Processing action: {userAction}, item: {item.Id} in Cart: {ctx.Self} ");

            switch (userAction)
            {                
                case "add":                    
                    cart.Items.Add(item);
                    cart.TotalAmount += item.Price;
                    break;
                case "remove":
                    cart.Items.Remove(item);
                    cart.TotalAmount -= item.Price;
                    break;
            }
            ctx.SetState(cart);            
        }

        private static bool CheckForDuplicate(Cart cart, string eTag)
        {
            return cart.Items.Find(x => x.ETag == eTag) != null;
        }
    }
}