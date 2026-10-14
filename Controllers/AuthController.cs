using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Market.Domain.Services;
using Market.Models;
using Market.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Market.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [AllowAnonymous, HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterResource resource)
        {
            var user = await _authService.RegisterAsync(resource.Email, resource.Password, UserRoles.Customer);
            if (user == null)
                return Conflict(new { message = "An account with this email already exists." });

            await SignInAsync(user);
            return StatusCode(201, new { user.Id, user.Email, user.Role });
        }

        [AllowAnonymous, HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginResource resource)
        {
            var user = await _authService.FindByEmailAsync(resource.Email);
            if (user == null || !_authService.VerifyPassword(user, resource.Password))
                return Unauthorized(new { message = "Email or password is incorrect." });

            await SignInAsync(user);
            return Ok(new { user.Id, user.Email, user.Role });
        }

        [Authorize, HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new { id = User.FindFirstValue(ClaimTypes.NameIdentifier), email = User.FindFirstValue(ClaimTypes.Email), role = User.FindFirstValue(ClaimTypes.Role) });
        }

        [Authorize, HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }

        [AllowAnonymous, HttpPost("bootstrap-admin")]
        public async Task<IActionResult> BootstrapAdminAsync(BootstrapAdminResource resource)
        {
            var configuredKey = _configuration["Bootstrap:AdminKey"];
            if (string.IsNullOrWhiteSpace(configuredKey) || !SecureEquals(configuredKey, resource.BootstrapKey))
                return Unauthorized();
            if (await _authService.HasAdministratorAsync())
                return Conflict(new { message = "An administrator already exists." });

            var user = await _authService.RegisterAsync(resource.Email, resource.Password, UserRoles.Admin);
            if (user == null)
                return Conflict(new { message = "An account with this email already exists." });

            await SignInAsync(user);
            return StatusCode(201, new { user.Id, user.Email, user.Role });
        }

        [Authorize(Roles = UserRoles.Admin), HttpPost("cashiers")]
        public async Task<IActionResult> CreateCashierAsync(CreateCashierResource resource)
        {
            var user = await _authService.RegisterAsync(resource.Email, resource.Password, UserRoles.Cashier);
            if (user == null)
                return Conflict(new { message = "An account with this email already exists." });
            return StatusCode(201, new { user.Id, user.Email, user.Role });
        }

        private async Task SignInAsync(MarketUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        private static bool SecureEquals(string left, string right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left);
            var rightBytes = Encoding.UTF8.GetBytes(right);
            return leftBytes.Length == rightBytes.Length && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }
    }
}