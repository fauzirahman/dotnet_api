using System;
using System.Linq;
using System.Threading.Tasks;
using Market.Models;
using Market.Resources;
using Market.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.Controllers
{
    [ApiController]
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Cashier)]
    [Route("api/reports")]
    public class ReportsController : Controller
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("stock")]
        public async Task<IActionResult> StockAsync([FromQuery] int threshold = 5)
        {
            if (threshold < 0 || threshold > 1000000)
                return BadRequest(new { message = "Threshold must be between 0 and 1000000." });
            return Ok(await _reportService.GetStockAsync(threshold));
        }

        [HttpGet("sales")]
        public async Task<IActionResult> SalesAsync([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            if (!TryGetRange(fromDate, toDate, out var fromUtc, out var toUtcExclusive))
                return BadRequest(new { message = "Date range is invalid; provide fromDate and toDate in YYYY-MM-DD format." });
            return Ok(await _reportService.GetSalesAsync(fromUtc, toUtcExclusive));
        }

        [HttpGet("orders")]
        public async Task<IActionResult> OrdersAsync([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            if (!TryGetRange(fromDate, toDate, out var fromUtc, out var toUtcExclusive))
                return BadRequest(new { message = "Date range is invalid; provide fromDate and toDate in YYYY-MM-DD format." });

            var result = await _reportService.GetOrdersAsync(fromUtc, toUtcExclusive);
            var orders = result.Orders.Select(order => new OrderResource
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerEmail = order.Customer.Email,
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
            });
            return Ok(new OrdersReportResource
            {
                FromUtc = fromUtc,
                ToUtcExclusive = toUtcExclusive,
                CountsByStatus = result.Counts,
                Orders = orders
            });
        }

        private static bool TryGetRange(DateOnly? fromDate, DateOnly? toDate, out DateTime fromUtc, out DateTime toUtcExclusive)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var from = fromDate ?? today.AddDays(-29);
            var to = toDate ?? today;
            fromUtc = DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            toUtcExclusive = DateTime.SpecifyKind(to.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            return to >= from && to.DayNumber - from.DayNumber <= 366;
        }
    }
}