using Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Tests.Core.ValueObjects;

public class PasswordTests
{
    [Fact]
    public void Constructor_WithValidPassword_CreatesHashedPassword()
    {
        // Arrange
        var validPassword = "Pass@123";

        // Act
        var password = new Password(validPassword);

        // Assert
        Assert.NotNull(password.Hashed);
        Assert.NotEmpty(password.Hashed);
        Assert.NotEqual(validPassword, password.Hashed); // Hash deve ser diferente da senha original
    }

    [Theory]
    [InlineData("Pass@12")] // Menos de 8 caracteres
    [InlineData("Pass@1")]
    [InlineData("Short1!")]
    public void Constructor_WithPasswordLessThan8Characters_ThrowsValidationException(string shortPassword)
    {
        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => new Password(shortPassword));
        Assert.Equal("A senha deve conter ao menos 8 caracteres, um número, uma letra e um caractere especial.", exception.Message);
    }

    [Theory]
    [InlineData("Password@")] // Sem número
    [InlineData("TestPassword!")]
    public void Constructor_WithPasswordWithoutNumber_ThrowsValidationException(string passwordWithoutNumber)
    {
        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => new Password(passwordWithoutNumber));
        Assert.Equal("A senha deve conter ao menos 8 caracteres, um número, uma letra e um caractere especial.", exception.Message);
    }

    [Theory]
    [InlineData("12345678@")] // Sem letra
    [InlineData("123456789!")]
    public void Constructor_WithPasswordWithoutLetter_ThrowsValidationException(string passwordWithoutLetter)
    {
        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => new Password(passwordWithoutLetter));
        Assert.Equal("A senha deve conter ao menos 8 caracteres, um número, uma letra e um caractere especial.", exception.Message);
    }

    [Theory]
    [InlineData("Password123")] // Sem caractere especial
    [InlineData("TestPass123")]
    public void Constructor_WithPasswordWithoutSpecialCharacter_ThrowsValidationException(string passwordWithoutSpecial)
    {
        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => new Password(passwordWithoutSpecial));
        Assert.Equal("A senha deve conter ao menos 8 caracteres, um número, uma letra e um caractere especial.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyPassword_ThrowsValidationException(string invalidPassword)
    {
        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => new Password(invalidPassword));
        Assert.Equal("A senha deve conter ao menos 8 caracteres, um número, uma letra e um caractere especial.", exception.Message);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsSuccess()
    {
        // Arrange
        var rawPassword = "Pass@123";
        var password = new Password(rawPassword);

        // Act
        var result = password.Verify(rawPassword);

        // Assert
        Assert.Equal(PasswordVerificationResult.Success, result);
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ReturnsFailed()
    {
        // Arrange
        var rawPassword = "Pass@123";
        var wrongPassword = "Wrong@123";
        var password = new Password(rawPassword);

        // Act
        var result = password.Verify(wrongPassword);

        // Assert
        Assert.Equal(PasswordVerificationResult.Failed, result);
    }

    [Fact]
    public void FromHashed_CreatesPasswordFromExistingHash()
    {
        // Arrange
        var rawPassword = "Pass@123";
        var originalPassword = new Password(rawPassword);
        var hashedValue = originalPassword.Hashed;

        // Act
        var passwordFromHash = Password.FromHashed(hashedValue);

        // Assert
        Assert.Equal(hashedValue, passwordFromHash.Hashed);
    }

    [Fact]
    public void FromHashed_VerifyWorksWithRestoredPassword()
    {
        // Arrange
        var rawPassword = "Pass@123";
        var originalPassword = new Password(rawPassword);
        var hashedValue = originalPassword.Hashed;
        var restoredPassword = Password.FromHashed(hashedValue);

        // Act
        var result = restoredPassword.Verify(rawPassword);

        // Assert
        Assert.Equal(PasswordVerificationResult.Success, result);
    }

    [Fact]
    public void Equals_WithSameHash_ReturnsTrue()
    {
        // Arrange
        var rawPassword = "Pass@123";
        var password1 = new Password(rawPassword);
        var password2 = Password.FromHashed(password1.Hashed);

        // Act
        var result = password1.Equals(password2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentHash_ReturnsFalse()
    {
        // Arrange
        var password1 = new Password("Pass@123");
        var password2 = new Password("Other@456");

        // Act
        var result = password1.Equals(password2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithNonPasswordObject_ReturnsFalse()
    {
        // Arrange
        var password = new Password("Pass@123");
        var otherObject = "some string";

        // Act
        var result = password.Equals(otherObject);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_ReturnsSameValueForSameHash()
    {
        // Arrange
        var rawPassword = "Pass@123";
        var password1 = new Password(rawPassword);
        var password2 = Password.FromHashed(password1.Hashed);

        // Act
        var hashCode1 = password1.GetHashCode();
        var hashCode2 = password2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ReturnsDifferentValueForDifferentHash()
    {
        // Arrange
        var password1 = new Password("Pass@123");
        var password2 = new Password("Other@456");

        // Act
        var hashCode1 = password1.GetHashCode();
        var hashCode2 = password2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }
}
