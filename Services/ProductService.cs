using System.Collections.Generic;
using System.Threading.Tasks;
using Market.Models;
using Market.Domain.Repositories;
using Market.Domain.Services;


namespace Market.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
    
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> ListAsync()
        {
            return await _productRepository.ListAsync();
        }
    }
}