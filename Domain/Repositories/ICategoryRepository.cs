using System.Collections.Generic;
using System.Threading.Tasks;
using Market.Models;
using Market.Domain.Communication;


namespace Market.Domain.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> ListAsync();
	    Task AddAsync(Category category);        
        Task<Category> FindByIdAsync(int id);
        void Remove(Category category);
        void Update(Category category);
    }
}