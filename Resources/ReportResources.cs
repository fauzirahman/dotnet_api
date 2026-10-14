using System;
using System.Collections.Generic;

namespace Market.Resources
{
    public class StockReportResource
    {
        public int ProductCount { get; set; }
        public int TotalUnits { get; set; }
        public decimal InventoryValue { get; set; }
        public int LowStockThreshold { get; set; }
        public IEnumerable<StockReportItemResource> Products { get; set; }
    }

    public class StockReportItemResource
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int StockQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal StockValue { get; set; }
    }

    public class SalesReportResource
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtcExclusive { get; set; }
        public int PaidOrderCount { get; set; }
        public decimal Revenue { get; set; }
        public IEnumerable<DailySalesResource> Daily { get; set; }
    }

    public class DailySalesResource
    {
        public DateTime DateUtc { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class OrdersReportResource
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtcExclusive { get; set; }
        public IDictionary<string, int> CountsByStatus { get; set; }
        public IEnumerable<OrderResource> Orders { get; set; }
    }
}