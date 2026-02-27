using Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities;
public class User
{
    public int Id { get; set; }
    public Email Email { get; set; }
    public Password Password { get; set; }
    public DateTime CreatedAt { get; set; }

    public User(Email email, Password password)
    {
        Email = email;
        Password = password;
    }

    public bool VerifyPassword(string plainTextPassword) 
        => GetPasswordVerificationResult(plainTextPassword) != PasswordVerificationResult.Failed;

    public PasswordVerificationResult GetPasswordVerificationResult(string plainTextPassword)
            => Password.Verify(plainTextPassword);

    public void ChangePassword(string currentPassword, string newPassword)
    {
        if (!GetPasswordVerificationResult(currentPassword).Equals(PasswordVerificationResult.Success))
            throw new ValidationException("Senha inválida.");

        if (GetPasswordVerificationResult(newPassword).Equals(PasswordVerificationResult.Success))
            throw new ValidationException("A nova senha não pode ser igual a senha atual.");

        UpdatePassword(new Password(newPassword));
    }

    public void ForceChangePassword(Password password)
    {
        UpdatePassword(password);
    }

    private void UpdatePassword(Password password)
    {
        Password = password;
    }
}
