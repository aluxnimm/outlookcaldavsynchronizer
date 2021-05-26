using System;

namespace CalDavSynchronizer.OAuth.Daimler.Models
{
    public class TokenData
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime Expiration { get; set; }

        public string Username { get; set; }

        public bool IsExpired => Expiration.ToUniversalTime() < DateTime.UtcNow;
    }
}
