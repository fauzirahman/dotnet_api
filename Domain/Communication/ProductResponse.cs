using Market.Models;

namespace Market.Domain.Communication
{
    public class ProductResponse : BaseResponse
    {
        public Product Product { get; private set; }

        public ProductResponse(Product product) : base(true, string.Empty)
        {
            Product = product;
        }

        public ProductResponse(string message) : base(false, message)
        {
        }
    }
}