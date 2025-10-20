namespace Order.API.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int ProcutId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
