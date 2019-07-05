using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Cart
{
    public static class CartProcessor
    {
        [FunctionName("Cart")]
        public static void Counter(
            [EntityTrigger(EntityName = "Cart")] IDurableEntityContext ctx)
        {
            int currentValue = ctx.GetState<int>();
            int operand = ctx.GetInput<int>();

            switch (ctx.OperationName)
            {
                case "add":
                    currentValue += operand;
                    break;
                case "subtract":
                    currentValue -= operand;
                    break;
                case "reset":
                    //await SendResetNotificationAsync();
                    currentValue = 0;
                    break;
            }

            ctx.SetState(currentValue);
        }
    }
}
