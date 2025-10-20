namespace Order.API.Viewmodels
{
    public class VM_CreateOrder
    {
        public int BuyerId { get; set; }
        public List<VM_OrderItem> OrderItems { get; set; }
    }
}
