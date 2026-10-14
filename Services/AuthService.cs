using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Market.Domain.Services;
using Market.Models;
using Market.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Market.Services
{
    public class AuthService : IAuthService
    {
        private const int Iterations = 210000;
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public Task<MarketUser> FindByEmailAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);
            return _context.Users.SingleOrDefaultAsync(user => user.Email == normalizedEmail);
        }

        public async Task<MarketUser> RegisterAsync(string email, string password, string role)
        {
            var normalizedEmail = NormalizeEmail(email);
            if (await _context.Users.AnyAsync(user => user.Email == normalizedEmail))
                return null;

            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
            var user = new MarketUser
            {
                Email = normalizedEmail,
                PasswordHash = $"{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}",
                Role = role,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public bool VerifyPassword(MarketUser user, string password)
        {
            var parts = user.PasswordHash.Split(':');
            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
                return false;

            try
            {
                var salt = Convert.FromBase64String(parts[1]);
                var expected = Convert.FromBase64String(parts[2]);
                var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public Task<bool> HasAdministratorAsync()
        {
            return _context.Users.AnyAsync(user => user.Role == UserRoles.Admin);
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    }
}