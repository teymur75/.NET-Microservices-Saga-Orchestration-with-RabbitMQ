using Microsoft.EntityFrameworkCore;
using M= Order.API.Models;

namespace Order.API.Contexts
{
    public class OrderContext : DbContext
    {
        public OrderContext(DbContextOptions options) : base(options)
        {
        }
       
        public DbSet<M.Order> Orders { get; set; }
        public DbSet<M.OrderItem> OrderItems { get; set; }
    }
}
