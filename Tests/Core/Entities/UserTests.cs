using Core.Entities;
using Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Tests.Core.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidEmailAndPassword_CreatesUser()
    {
        // Arrange
        var email = new Email("test@example.com");
        var password = new Password("Pass@123");

        // Act
        var user = new User(email, password);

        // Assert
        Assert.Equal(email, user.Email);
        Assert.Equal(password, user.Password);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var rawPassword = "Pass@123";
        var email = new Email("test@example.com");
        var password = new Password(rawPassword);
        var user = new User(email, password);

        // Act
        var result = user.VerifyPassword(rawPassword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var rawPassword = "Pass@123";
        var wrongPassword = "Wrong@123";
        var email = new Email("test@example.com");
        var password = new Password(rawPassword);
        var user = new User(email, password);

        // Act
        var result = user.VerifyPassword(wrongPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetPasswordVerificationResult_WithCorrectPassword_ReturnsSuccess()
    {
        // Arrange
        var rawPassword = "Pass@123";
        var email = new Email("test@example.com");
        var password = new Password(rawPassword);
        var user = new User(email, password);

        // Act
        var result = user.GetPasswordVerificationResult(rawPassword);

        // Assert
        Assert.Equal(PasswordVerificationResult.Success, result);
    }

    [Fact]
    public void GetPasswordVerificationResult_WithIncorrectPassword_ReturnsFailed()
    {
        // Arrange
        var rawPassword = "Pass@123";
        var wrongPassword = "Wrong@123";
        var email = new Email("test@example.com");
        var password = new Password(rawPassword);
        var user = new User(email, password);

        // Act
        var result = user.GetPasswordVerificationResult(wrongPassword);

        // Assert
        Assert.Equal(PasswordVerificationResult.Failed, result);
    }

    [Fact]
    public void ChangePassword_WithCorrectCurrentPassword_UpdatesPassword()
    {
        // Arrange
        var currentPassword = "Pass@123";
        var newPassword = "NewPass@456";
        var email = new Email("test@example.com");
        var password = new Password(currentPassword);
        var user = new User(email, password);

        // Act
        user.ChangePassword(currentPassword, newPassword);

        // Assert
        Assert.True(user.VerifyPassword(newPassword));
        Assert.False(user.VerifyPassword(currentPassword));
    }

    [Fact]
    public void ChangePassword_WithIncorrectCurrentPassword_ThrowsValidationException()
    {
        // Arrange
        var currentPassword = "Pass@123";
        var wrongPassword = "Wrong@123";
        var newPassword = "NewPass@456";
        var email = new Email("test@example.com");
        var password = new Password(currentPassword);
        var user = new User(email, password);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            user.ChangePassword(wrongPassword, newPassword));
        Assert.Equal("Senha inválida.", exception.Message);
    }

    [Fact]
    public void ChangePassword_WithNewPasswordSameAsCurrent_ThrowsValidationException()
    {
        // Arrange
        var currentPassword = "Pass@123";
        var email = new Email("test@example.com");
        var password = new Password(currentPassword);
        var user = new User(email, password);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => 
            user.ChangePassword(currentPassword, currentPassword));
        Assert.Equal("A nova senha não pode ser igual a senha atual.", exception.Message);
    }

    [Fact]
    public void ForceChangePassword_UpdatesPasswordWithoutValidation()
    {
        // Arrange
        var currentPassword = "Pass@123";
        var newPasswordRaw = "NewPass@456";
        var email = new Email("test@example.com");
        var password = new Password(currentPassword);
        var user = new User(email, password);
        var newPassword = new Password(newPasswordRaw);

        // Act
        user.ForceChangePassword(newPassword);

        // Assert
        Assert.True(user.VerifyPassword(newPasswordRaw));
        Assert.False(user.VerifyPassword(currentPassword));
    }

    [Fact]
    public void ForceChangePassword_DoesNotRequireCurrentPassword()
    {
        // Arrange
        var currentPassword = "Pass@123";
        var newPasswordRaw = "NewPass@456";
        var email = new Email("test@example.com");
        var password = new Password(currentPassword);
        var user = new User(email, password);
        var newPassword = new Password(newPasswordRaw);

        // Act
        user.ForceChangePassword(newPassword);

        // Assert
        Assert.Equal(newPassword, user.Password);
    }

    [Fact]
    public void User_HasIdProperty()
    {
        // Arrange
        var email = new Email("test@example.com");
        var password = new Password("Pass@123");
        var user = new User(email, password);

        // Act
        user.Id = 123;

        // Assert
        Assert.Equal(123, user.Id);
    }

    [Fact]
    public void User_HasCreatedAtProperty()
    {
        // Arrange
        var email = new Email("test@example.com");
        var password = new Password("Pass@123");
        var user = new User(email, password);
        var createdAt = DateTime.UtcNow;

        // Act
        user.CreatedAt = createdAt;

        // Assert
        Assert.Equal(createdAt, user.CreatedAt);
    }
}
