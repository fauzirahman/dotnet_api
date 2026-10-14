using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Market.Models;
using Market.Persistence.Contexts;
using Market.Resources;
using Microsoft.EntityFrameworkCore;

namespace Market.Services
{
    public class ReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StockReportResource> GetStockAsync(int threshold)
        {
            var products = await _context.Products.AsNoTracking().Include(product => product.Category)
                .OrderBy(product => product.StockQuantity).ThenBy(product => product.Name).ToListAsync();
            return new StockReportResource
            {
                ProductCount = products.Count,
                TotalUnits = products.Sum(product => product.StockQuantity),
                InventoryValue = products.Sum(product => product.Price * product.StockQuantity),
                LowStockThreshold = threshold,
                Products = products.Select(product => new StockReportItemResource
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    CategoryName = product.Category.Name,
                    StockQuantity = product.StockQuantity,
                    Price = product.Price,
                    StockValue = product.Price * product.StockQuantity
                })
            };
        }

        public async Task<SalesReportResource> GetSalesAsync(DateTime fromUtc, DateTime toUtcExclusive)
        {
            var paidOrders = _context.Orders.AsNoTracking().Where(order => order.CreatedAtUtc >= fromUtc && order.CreatedAtUtc < toUtcExclusive && order.PaymentStatus == "Paid" && order.Status != "Cancelled");
            var orderCount = await paidOrders.CountAsync();
            var revenue = orderCount == 0 ? 0 : await paidOrders.SumAsync(order => order.Total);
            var daily = await paidOrders.GroupBy(order => order.CreatedAtUtc.Date)
                .Select(group => new DailySalesResource
                {
                    DateUtc = group.Key,
                    OrderCount = group.Count(),
                    Revenue = group.Sum(order => order.Total)
                }).OrderBy(day => day.DateUtc).ToListAsync();

            return new SalesReportResource
            {
                FromUtc = fromUtc,
                ToUtcExclusive = toUtcExclusive,
                PaidOrderCount = orderCount,
                Revenue = revenue,
                Daily = daily
            };
        }

        public async Task<(IReadOnlyList<MarketOrder> Orders, IDictionary<string, int> Counts)> GetOrdersAsync(DateTime fromUtc, DateTime toUtcExclusive)
        {
            var query = _context.Orders.AsNoTracking().Include(order => order.Customer).Include(order => order.Items)
                .Where(order => order.CreatedAtUtc >= fromUtc && order.CreatedAtUtc < toUtcExclusive);
            var counts = await query.GroupBy(order => order.Status)
                .Select(group => new { Status = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.Status, item => item.Count);
            var orders = await query.OrderByDescending(order => order.CreatedAtUtc).ToListAsync();
            return (orders, counts);
        }
    }
}