using System;
using System.Collections.Generic;

namespace Market.Models
{
    public class MarketOrder
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int CustomerId { get; set; }
        public MarketUser Customer { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public decimal Total { get; set; }
        public IList<MarketOrderItem> Items { get; set; } = new List<MarketOrderItem>();
    }
}