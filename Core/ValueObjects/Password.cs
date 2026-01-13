using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Core.ValueObjects;
public class Password
{
    private static readonly PasswordHasher<object> _hasher = new();

    public string Hashed { get; }

    public Password(string rawPassword)
    {
        if (!IsValid(rawPassword))
            throw new ValidationException("A senha deve conter ao menos 8 caracteres, um número, uma letra e um caractere especial.");

        Hashed = _hasher.HashPassword(null, rawPassword);
    }

    private Password(string hashedPassword, bool alreadyHashed)
    {
        Hashed = hashedPassword;
    }

    public static Password FromHashed(string hashedPassword)
    {
        return new Password(hashedPassword, true);
    }

    public PasswordVerificationResult Verify(string plainTextPassword)
        => _hasher.VerifyHashedPassword(null, Hashed, plainTextPassword);

    private static bool IsValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        if (password.Length < 8) return false;
        if (!password.Any(char.IsDigit)) return false;
        if (!password.Any(char.IsLetter)) return false;
        if (!password.Any(ch => !char.IsLetterOrDigit(ch))) return false;
        return true;
    }

    public override bool Equals(object obj)
    {
        if (obj is Password other)
            return Hashed == other.Hashed;
        return false;
    }

    public override int GetHashCode() => Hashed.GetHashCode();
}
