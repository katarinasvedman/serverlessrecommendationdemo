using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CartView
{
    public static class CartDurableEntity
    {
        [FunctionName("Cart")]
        public static void Cart(
            [EntityTrigger(EntityName = "Cart")] IDurableEntityContext ctx,
            ILogger log)
        {
            var userAction = ctx.OperationName;

            //If Cart doesn't exist it will initialize with an empty item list
            var cart = ctx.GetState(() => new Cart(ctx.EntityKey));

            switch (userAction)
            {
                case "add":
                    var item = ctx.GetInput<Item>();
                    //Duplicate detection. Don't add duplicates.
                    if (CheckForDuplicate(cart, item.ETag))
                        return;
                    cart.Items.Add(ctx.GetInput<Item>());
                    cart.TotalAmount += item.Price;
                    log.LogInformation($"Added item in cart: {item.Id} in Cart: {ctx.ToString()}");
                    break;
                case "remove":
                    item = ctx.GetInput<Item>();
                    cart.Items.Remove(item);
                    cart.TotalAmount -= item.Price;
                    log.LogInformation($"Removed item in cart: {item.Id} in Cart: {ctx.ToString()}");
                    break;
                case "purchased":
                    //Checked out - Cart is no longer needed 
                    log.LogInformation($"Cart is destructed: {ctx.ToString()}");
                    ctx.DeleteState();
                    return;
                default:
                    return;
            }
            ctx.SetState(cart);
        }

        private static bool CheckForDuplicate(Cart cart, string eTag)
        {
            return cart.Items.Any(x => x.ETag == eTag);
        }
    }
}