using System.Collections.Generic;
using System.Threading.Tasks;
using Market.Models;

namespace Market.Domain.Repositories
{
    public interface IProductRepository
    {
         Task<IEnumerable<Product>> ListAsync();
            Task<Product> FindByIdAsync(int id);
            Task AddAsync(Product product);
            void Update(Product product);
            void Remove(Product product);
    }
}