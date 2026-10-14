using System.Collections.Generic;
using System.Threading.Tasks;
using Market.Domain.Communication;
using Market.Models;

namespace Market.Domain.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> PlaceAsync(int customerId, IEnumerable<(int ProductId, int Quantity)> items, string paymentMethod);
        Task<IReadOnlyList<MarketOrder>> ListAsync(int? customerId);
        Task<MarketOrder> FindAsync(int id, int? customerId);
        Task<OrderResponse> UpdateStatusAsync(int id, string status);
        Task<OrderResponse> MarkPaidAsync(int id);
    }
}