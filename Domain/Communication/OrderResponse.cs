using Market.Models;

namespace Market.Domain.Communication
{
    public class OrderResponse : BaseResponse
    {
        public MarketOrder Order { get; private set; }

        public OrderResponse(MarketOrder order) : base(true, string.Empty)
        {
            Order = order;
        }

        public OrderResponse(string message) : base(false, message)
        {
        }
    }
}