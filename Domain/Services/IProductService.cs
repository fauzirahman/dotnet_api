using System.Collections.Generic;
using System.Threading.Tasks;
using Market.Models;

namespace Market.Domain.Services
{
    public interface IProductService
    {
         Task<IEnumerable<Product>> ListAsync();
    }
}