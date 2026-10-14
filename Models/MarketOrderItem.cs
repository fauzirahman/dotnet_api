namespace Market.Models
{
    public class MarketOrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public MarketOrder Order { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}