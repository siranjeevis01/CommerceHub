using BCrypt.Net;
using CommerceHub.Modules.Identity.Application.Common.Interfaces;

namespace CommerceHub.Modules.Identity.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string storedHash)
    {
        try
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            if (storedHash.Contains(':'))
            {
                var parts = storedHash.Split(':');
                if (parts.Length == 2)
                {
                    var salt = parts[0];
                    var hash = parts[1];
                    var saltBytes = Convert.FromBase64String(salt);
                    using var hmac = new System.Security.Cryptography.HMACSHA512(saltBytes);
                    var computedHash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
                    return computedHash == hash;
                }
            }

            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
        catch
        {
            return false;
        }
    }
}
