namespace EcomWebsite.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string? Status { get; set; }          // ✅ nullable
        public string? PaymentId { get; set; }       // ✅ nullable
        public string? PaymentStatus { get; set; }   // ✅ nullable
        public string? ShippingAddress { get; set; } // ✅ nullable

   
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
