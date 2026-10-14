using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Market.Domain.Services;
using Market.Models;
using Market.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/orders")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize(Roles = UserRoles.Customer), HttpPost]
        public async Task<IActionResult> PlaceAsync(PlaceOrderResource resource)
        {
            var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _orderService.PlaceAsync(customerId, resource.Items.Select(item => (item.ProductId, item.Quantity)), resource.PaymentMethod);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return StatusCode(201, ToResource(result.Order));
        }

        [HttpGet]
        public async Task<IActionResult> ListAsync()
        {
            var isStaff = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Cashier);
            var customerId = isStaff ? (int?)null : int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var orders = await _orderService.ListAsync(customerId);
            return Ok(orders.Select(ToResource));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var isStaff = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Cashier);
            var customerId = isStaff ? (int?)null : int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var order = await _orderService.FindAsync(id, customerId);
            return order == null ? NotFound() : Ok(ToResource(order));
        }

        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Cashier), HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatusAsync(int id, UpdateOrderStatusResource resource)
        {
            var result = await _orderService.UpdateStatusAsync(id, resource.Status);
            if (!result.Success)
                return Conflict(new { message = result.Message });
            return Ok(ToResource(result.Order));
        }

        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Cashier), HttpPut("{id}/payment")]
        public async Task<IActionResult> MarkPaidAsync(int id)
        {
            var result = await _orderService.MarkPaidAsync(id);
            if (!result.Success)
                return Conflict(new { message = result.Message });
            return Ok(ToResource(result.Order));
        }

        [Authorize(Roles = UserRoles.Customer), HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelAsync(int id)
        {
            var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var order = await _orderService.FindAsync(id, customerId);
            if (order == null)
                return NotFound();
            if (order.Status != "Pending")
                return Conflict(new { message = "Only pending orders can be cancelled by the customer." });
            var result = await _orderService.UpdateStatusAsync(id, "Cancelled");
            if (!result.Success)
                return Conflict(new { message = result.Message });
            return Ok(ToResource(result.Order));
        }

        private static OrderResource ToResource(MarketOrder order)
        {
            return new OrderResource
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerEmail = order.Customer?.Email,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                Total = order.Total,
                CreatedAtUtc = order.CreatedAtUtc,
                Items = order.Items.Select(item => new OrderItemResource
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                })
            };
        }
    }
}