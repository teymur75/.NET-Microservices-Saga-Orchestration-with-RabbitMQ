namespace Shared.Message
{
    public class StockRollBackMessage
    {
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
