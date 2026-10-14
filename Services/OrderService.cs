using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Market.Domain.Communication;
using Market.Domain.Services;
using Market.Models;
using Market.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Market.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderResponse> PlaceAsync(int customerId, IEnumerable<(int ProductId, int Quantity)> requestedItems, string paymentMethod)
        {
            var lines = requestedItems.ToList();
            if (lines.Count == 0 || lines.GroupBy(line => line.ProductId).Any(group => group.Count() > 1))
                return new OrderResponse("Order must contain unique products.");

            var productIds = lines.Select(line => line.ProductId).ToArray();
            var products = await _context.Products.Where(product => productIds.Contains(product.Id)).ToDictionaryAsync(product => product.Id);
            if (products.Count != productIds.Length)
                return new OrderResponse("One or more products were not found.");

            foreach (var line in lines)
            {
                if (line.Quantity < 1 || products[line.ProductId].StockQuantity < line.Quantity)
                    return new OrderResponse($"Insufficient stock for {products[line.ProductId].Name}.");
            }

            var order = new MarketOrder
            {
                OrderNumber = $"MKT-{Guid.NewGuid():N}"[..32],
                CustomerId = customerId,
                CreatedAtUtc = DateTime.UtcNow,
                Status = "Pending",
                PaymentMethod = paymentMethod,
                PaymentStatus = "Unpaid",
                Items = lines.Select(line => new MarketOrderItem
                {
                    ProductId = line.ProductId,
                    ProductName = products[line.ProductId].Name,
                    Quantity = line.Quantity,
                    UnitPrice = products[line.ProductId].Price
                }).ToList()
            };
            order.Total = order.Items.Sum(item => item.UnitPrice * item.Quantity);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            foreach (var line in lines)
            {
                var updated = await _context.Products
                    .Where(product => product.Id == line.ProductId && product.StockQuantity >= line.Quantity && product.Price == products[line.ProductId].Price)
                    .ExecuteUpdateAsync(update => update.SetProperty(product => product.StockQuantity, product => product.StockQuantity - line.Quantity));
                if (updated != 1)
                    return new OrderResponse($"Stock or price changed for {products[line.ProductId].Name}. Refresh and retry.");
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OrderResponse(order);
        }

        public async Task<IReadOnlyList<MarketOrder>> ListAsync(int? customerId)
        {
            var query = _context.Orders.AsNoTracking().Include(order => order.Customer).Include(order => order.Items).AsQueryable();
            if (customerId.HasValue)
                query = query.Where(order => order.CustomerId == customerId.Value);
            return await query.OrderByDescending(order => order.CreatedAtUtc).ToListAsync();
        }

        public Task<MarketOrder> FindAsync(int id, int? customerId)
        {
            var query = _context.Orders.AsNoTracking().Include(order => order.Customer).Include(order => order.Items).Where(order => order.Id == id);
            if (customerId.HasValue)
                query = query.Where(order => order.CustomerId == customerId.Value);
            return query.SingleOrDefaultAsync();
        }

        public async Task<OrderResponse> UpdateStatusAsync(int id, string status)
        {
            var order = await _context.Orders.Include(item => item.Items).SingleOrDefaultAsync(item => item.Id == id);
            if (order == null)
                return new OrderResponse("Order not found.");

            var allowed = (order.Status, status) switch
            {
                ("Pending", "Confirmed") => true,
                ("Pending", "Cancelled") => true,
                ("Confirmed", "Shipped") => true,
                ("Confirmed", "Cancelled") => true,
                ("Shipped", "Delivered") => true,
                _ => false
            };
            if (!allowed)
                return new OrderResponse($"Cannot move order from {order.Status} to {status}.");
            if (status == "Cancelled" && order.PaymentStatus == "Paid")
                return new OrderResponse("A paid order cannot be cancelled without a refund workflow.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            if (status == "Cancelled")
            {
                foreach (var item in order.Items)
                {
                    await _context.Products.Where(product => product.Id == item.ProductId)
                        .ExecuteUpdateAsync(update => update.SetProperty(product => product.StockQuantity, product => product.StockQuantity + item.Quantity));
                }
            }

            order.Status = status;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return new OrderResponse("Order was updated by another request. Refresh and try again.");
            }
            await transaction.CommitAsync();
            return new OrderResponse(order);
        }

        public async Task<OrderResponse> MarkPaidAsync(int id)
        {
            var order = await _context.Orders.SingleOrDefaultAsync(item => item.Id == id);
            if (order == null)
                return new OrderResponse("Order not found.");
            if (order.Status == "Cancelled" || order.PaymentStatus == "Paid")
                return new OrderResponse("This order cannot be marked as paid.");
            if (order.Status != "Delivered")
                return new OrderResponse("COD payment can be recorded after delivery.");

            order.PaymentStatus = "Paid";
            await _context.SaveChangesAsync();
            return new OrderResponse(order);
        }
    }
}