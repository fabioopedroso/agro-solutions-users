using Core.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Tests.Core.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Constructor_WithValidEmail_CreatesEmailObject()
    {
        // Arrange
        var validEmail = "test@example.com";

        // Act
        var email = new Email(validEmail);

        // Assert
        Assert.Equal(validEmail, email.Address);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyEmail_ThrowsValidationException(string invalidEmail)
    {
        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => new Email(invalidEmail));
        Assert.Equal("O e-mail informado é inválido.", exception.Message);
    }

    [Theory]
    [InlineData("invalidemail")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    [InlineData("invalid.email")]
    public void Constructor_WithEmailWithoutAt_ThrowsValidationException(string invalidEmail)
    {
        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => new Email(invalidEmail));
        Assert.Equal("O e-mail informado é inválido.", exception.Message);
    }

    [Theory]
    [InlineData("test@domain")]
    [InlineData("test@domain.a")]
    public void Constructor_WithInvalidTLD_ThrowsValidationException(string invalidEmail)
    {
        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => new Email(invalidEmail));
        Assert.Equal("O e-mail informado é inválido.", exception.Message);
    }

    [Fact]
    public void ToString_ReturnsEmailAddress()
    {
        // Arrange
        var emailAddress = "test@example.com";
        var email = new Email(emailAddress);

        // Act
        var result = email.ToString();

        // Assert
        Assert.Equal(emailAddress, result);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("first.last@subdomain.example.com")]
    [InlineData("test123@test-domain.com")]
    public void IsValidEmail_WithValidEmails_ReturnsTrue(string validEmail)
    {
        // Act
        var result = Email.IsValidEmail(validEmail);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalidemail")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    [InlineData("test@domain")]
    [InlineData("test@domain.a")]
    public void IsValidEmail_WithInvalidEmails_ReturnsFalse(string invalidEmail)
    {
        // Act
        var result = Email.IsValidEmail(invalidEmail);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Constructor_Default_CreatesEmptyEmail()
    {
        // Act
        var email = new Email();

        // Assert
        Assert.Equal(string.Empty, email.Address);
    }
}
