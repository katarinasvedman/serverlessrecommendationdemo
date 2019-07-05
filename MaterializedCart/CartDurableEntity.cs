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
            //Checked out - Cart is no longer needed 
            if(ctx.OperationName == "purchased")
            {
                log.LogInformation($"Cart is destructed: {ctx.Self}");
                ctx.DestructOnExit();
                return;
            }

            //If Cart soesn't exist it will initialize with an empty item list
            Cart cart = ctx.GetState<Cart>( () =>  new Cart { CartId = ctx.Key, Items = new List<Item>() });

            //Read item from "request" input
            Item item = ctx.GetInput<Item>();
            if (item == null)
                ctx.Return(new Exception($"No Item found in request to Cart {ctx.Self}"));

            log.LogInformation($"Processing action: {ctx.OperationName}, item: {item.Id} in Cart: {ctx.Self} ");

            switch (ctx.OperationName)
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

    }
}