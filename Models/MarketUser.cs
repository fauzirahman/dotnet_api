using System;
using System.Collections.Generic;

namespace Market.Models
{
    public class MarketUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public IList<MarketOrder> Orders { get; set; } = new List<MarketOrder>();
    }
}