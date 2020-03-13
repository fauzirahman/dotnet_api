using System.Collections.Generic;
using System.Threading.Tasks;
using Market.Models;

namespace Market.Domain.Repositories
{
    public interface IProductRepository
    {
         Task<IEnumerable<Product>> ListAsync();
    }
}