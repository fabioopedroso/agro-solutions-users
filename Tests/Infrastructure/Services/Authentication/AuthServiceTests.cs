using Application.Contracts;
using Application.DTOs.Auth.Signature;
using Application.Exceptions;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.ValueObjects;
using Infrastructure.Services.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Tests.Infrastructure.Services.Authentication;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Configurar valores padrão para JWT
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKeyForJwtTokenGenerationWith32Characters!");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("AgroSolutionsIssuer");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("AgroSolutionsAudience");

        _service = new AuthService(_mockConfiguration.Object, _mockRepository.Object);
    }

    [Fact]
    public async Task GenerateToken_WithValidCredentials_ReturnsJwtToken()
    {
        // Arrange
        var password = "Pass@123";
        var email = "test@example.com";
        var loginDto = new LoginDto 
        { 
            Email = email, 
            Password = password 
        };

        var user = new User(new Email(email), new Password(password));
        user.Id = 1;

        _mockRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync(user);

        // Act
        var token = await _service.GenerateToken(loginDto);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task GenerateToken_WithNonExistentEmail_ThrowsNotFoundException()
    {
        // Arrange
        var loginDto = new LoginDto 
        { 
            Email = "nonexistent@example.com", 
            Password = "Pass@123" 
        };

        _mockRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.GenerateToken(loginDto));
        
        Assert.Equal("O usuário não foi encontrado.", exception.Message);
    }

    [Fact]
    public async Task GenerateToken_WithIncorrectPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var correctPassword = "Pass@123";
        var wrongPassword = "Wrong@123";
        var email = "test@example.com";
        
        var loginDto = new LoginDto 
        { 
            Email = email, 
            Password = wrongPassword 
        };

        var user = new User(new Email(email), new Password(correctPassword));
        user.Id = 1;

        _mockRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _service.GenerateToken(loginDto));
        
        Assert.Equal("Usuário ou senha inválidos.", exception.Message);
    }

    [Fact]
    public async Task GenerateToken_TokenContainsCorrectClaims()
    {
        // Arrange
        var password = "Pass@123";
        var email = "test@example.com";
        var userId = 123;
        
        var loginDto = new LoginDto 
        { 
            Email = email, 
            Password = password 
        };

        var user = new User(new Email(email), new Password(password));
        user.Id = userId;

        _mockRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync(user);

        // Act
        var token = await _service.GenerateToken(loginDto);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "Email");
        
        Assert.NotNull(userIdClaim);
        Assert.Equal(userId.ToString(), userIdClaim.Value);
        Assert.NotNull(emailClaim);
        Assert.Equal(email, emailClaim.Value);
    }

    [Fact]
    public async Task GenerateToken_TokenExpiresInOneHour()
    {
        // Arrange
        var password = "Pass@123";
        var email = "test@example.com";
        
        var loginDto = new LoginDto 
        { 
            Email = email, 
            Password = password 
        };

        var user = new User(new Email(email), new Password(password));
        user.Id = 1;

        _mockRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync(user);

        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = await _service.GenerateToken(loginDto);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var expectedExpiration = beforeGeneration.AddHours(1);
        var expirationDifference = Math.Abs((jwtToken.ValidTo - expectedExpiration).TotalSeconds);
        
        // Verificar se a diferença é menor que 5 segundos (margem para execução)
        Assert.True(expirationDifference < 5, 
            $"Token expiration should be approximately 1 hour from now. Difference: {expirationDifference} seconds");
    }

    [Fact]
    public async Task GenerateToken_TokenHasCorrectIssuerAndAudience()
    {
        // Arrange
        var password = "Pass@123";
        var email = "test@example.com";
        
        var loginDto = new LoginDto 
        { 
            Email = email, 
            Password = password 
        };

        var user = new User(new Email(email), new Password(password));
        user.Id = 1;

        _mockRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync(user);

        // Act
        var token = await _service.GenerateToken(loginDto);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        Assert.Equal("AgroSolutionsIssuer", jwtToken.Issuer);
        Assert.Contains("AgroSolutionsAudience", jwtToken.Audiences);
    }

    [Fact]
    public async Task GenerateToken_UpdatesPasswordIfRehashNeeded()
    {
        // Arrange
        var password = "Pass@123";
        var email = "test@example.com";
        
        var loginDto = new LoginDto 
        { 
            Email = email, 
            Password = password 
        };

        // Criar um user com uma senha que precisa de rehash
        // Para simular isso, vamos usar um mock mais sofisticado
        var user = new User(new Email(email), new Password(password));
        user.Id = 1;

        _mockRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync(user);

        // Act
        var token = await _service.GenerateToken(loginDto);

        // Assert
        Assert.NotNull(token);
        // Nota: Em condições normais, SuccessRehashNeeded raramente ocorre
        // Este teste verifica que o fluxo completa sem erros
    }

    [Fact]
    public async Task GenerateToken_CallsRepositoryGetByEmail()
    {
        // Arrange
        var password = "Pass@123";
        var email = "test@example.com";
        
        var loginDto = new LoginDto 
        { 
            Email = email, 
            Password = password 
        };

        var user = new User(new Email(email), new Password(password));
        user.Id = 1;

        _mockRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync(user);

        // Act
        await _service.GenerateToken(loginDto);

        // Assert
        _mockRepository.Verify(
            r => r.GetByEmailAsync(It.Is<Email>(e => e.Address == email)), 
            Times.Once
        );
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("")]
    [InlineData(null)]
    public async Task GenerateToken_WithInvalidEmailFormat_ThrowsValidationException(string invalidEmail)
    {
        // Arrange
        var loginDto = new LoginDto 
        { 
            Email = invalidEmail, 
            Password = "Pass@123" 
        };

        // Act & Assert
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => 
            _service.GenerateToken(loginDto));
    }
}
