using System.ComponentModel.DataAnnotations;

namespace Market.Resources
{
    public class RegisterResource
    {
        [Required, EmailAddress, MaxLength(254)]
        public string Email { get; set; }

        [Required, MinLength(10), MaxLength(128)]
        public string Password { get; set; }
    }

    public class LoginResource
    {
        [Required, EmailAddress, MaxLength(254)]
        public string Email { get; set; }

        [Required, MinLength(1), MaxLength(128)]
        public string Password { get; set; }
    }

    public class BootstrapAdminResource : RegisterResource
    {
        [Required]
        public string BootstrapKey { get; set; }
    }

    public class CreateCashierResource : RegisterResource
    {
    }
}