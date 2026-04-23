using System.Drawing;
using Microsoft.EntityFrameworkCore;
namespace EcomWebsite.Models
{
    public class myContext : DbContext
    {
        public myContext(DbContextOptions<myContext> Options) : base(Options)
        {
        }

        public DbSet<Admin> tbl_admin { get; set; }
        public DbSet<Customer> tbl_customer { get; set; }
        public DbSet<Category> tbl_category { get; set; }
        public DbSet<Product> tbl_product { get; set; }
        public DbSet<Cart> tbl_cart { get; set; }
        public DbSet<Feedback> tbl_feedback { get; set; }
        public DbSet<Faqs> tbl_faqs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Users> tbl_users { get; set; }
      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Product)
                .HasForeignKey(p => p.cat_id);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.product_price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(o => o.Price)
                .HasPrecision(18, 2);
        }
    }
}
