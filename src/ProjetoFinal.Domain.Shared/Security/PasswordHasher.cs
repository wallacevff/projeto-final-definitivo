using BCrypt.Net;

namespace ProjetoFinal.Domain.Shared.Security;

public static class PasswordHasher
{
    /// <summary>
    /// Creates a salted hash for the provided plain text password using BCrypt.
    /// </summary>
    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Verifies a plain text password against a previously generated hash.
    /// </summary>
    public static bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
