using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Market.Resources
{
    public class PlaceOrderResource
    {
        [Required, MinLength(1)]
        public List<PlaceOrderItemResource> Items { get; set; }

        [Required, RegularExpression("^COD$")]
        public string PaymentMethod { get; set; }
    }

    public class PlaceOrderItemResource
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, 10000)]
        public int Quantity { get; set; }
    }

    public class UpdateOrderStatusResource
    {
        [Required, RegularExpression("^(Confirmed|Shipped|Delivered|Cancelled)$")]
        public string Status { get; set; }
    }

    public class OrderItemResource
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class OrderResource
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerEmail { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public decimal Total { get; set; }
        public System.DateTime CreatedAtUtc { get; set; }
        public IEnumerable<OrderItemResource> Items { get; set; }
    }
}