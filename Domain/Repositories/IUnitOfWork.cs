using System.Threading.Tasks;

namespace Market.Domain.Repositories
{
    public interface IUnitOfWork
    {
         Task CompleteAsync();
    }
}