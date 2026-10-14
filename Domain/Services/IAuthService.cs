using System.Threading.Tasks;
using Market.Models;

namespace Market.Domain.Services
{
    public interface IAuthService
    {
        Task<MarketUser> FindByEmailAsync(string email);
        Task<MarketUser> RegisterAsync(string email, string password, string role);
        bool VerifyPassword(MarketUser user, string password);
        Task<bool> HasAdministratorAsync();
    }
}