using Application.Contracts;
using Application.DTOs.User.Signature;
using Application.Exceptions;
using Application.Services;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.ValueObjects;
using Moq;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Tests.Application.Services;

public class UserAppServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly UserAppService _service;

    public UserAppServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockCurrentUser = new Mock<ICurrentUserService>();
        _service = new UserAppService(_mockRepository.Object, _mockCurrentUser.Object);
    }

    [Fact]
    public async Task Register_WithUniqueEmail_CreatesUser()
    {
        // Arrange
        var dto = new RegisterDto 
        { 
            Email = "test@example.com", 
            Password = "Pass@123" 
        };
        
        _mockRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync((User?)null);

        // Act
        await _service.Register(dto);

        // Assert
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ThrowsConflictException()
    {
        // Arrange
        var dto = new RegisterDto 
        { 
            Email = "existing@example.com", 
            Password = "Pass@123" 
        };
        
        var existingUser = new User(
            new Email(dto.Email), 
            new Password(dto.Password)
        );
        
        _mockRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(() => 
            _service.Register(dto));
        
        Assert.Equal("O E-mail informado já está cadastrado.", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalidemail")]
    [InlineData("invalid@")]
    public async Task Register_WithInvalidEmail_ThrowsValidationException(string invalidEmail)
    {
        // Arrange
        var dto = new RegisterDto 
        { 
            Email = invalidEmail, 
            Password = "Pass@123" 
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _service.Register(dto));
        
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Theory]
    [InlineData("Short1!")]
    [InlineData("Password")]
    [InlineData("12345678")]
    [InlineData("Pass123")]
    public async Task Register_WithInvalidPassword_ThrowsValidationException(string invalidPassword)
    {
        // Arrange
        var dto = new RegisterDto 
        { 
            Email = "test@example.com", 
            Password = invalidPassword 
        };
        
        _mockRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _service.Register(dto));
        
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_WithValidData_UpdatesPassword()
    {
        // Arrange
        var currentPassword = "Pass@123";
        var newPassword = "NewPass@456";
        var userId = 1;
        
        var dto = new ChangePasswordDto 
        { 
            Password = currentPassword, 
            NewPassword = newPassword 
        };
        
        var user = new User(
            new Email("test@example.com"), 
            new Password(currentPassword)
        );
        user.Id = userId;
        
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        _mockRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        await _service.ChangePassword(dto);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
        Assert.True(user.VerifyPassword(newPassword));
    }

    [Fact]
    public async Task ChangePassword_WithNonExistentUser_ThrowsNotFoundException()
    {
        // Arrange
        var userId = 999;
        var dto = new ChangePasswordDto 
        { 
            Password = "Pass@123", 
            NewPassword = "NewPass@456" 
        };
        
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        _mockRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.ChangePassword(dto));
        
        Assert.Equal("Usuário não encontrado.", exception.Message);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_WithIncorrectCurrentPassword_ThrowsValidationException()
    {
        // Arrange
        var currentPassword = "Pass@123";
        var wrongPassword = "Wrong@123";
        var newPassword = "NewPass@456";
        var userId = 1;
        
        var dto = new ChangePasswordDto 
        { 
            Password = wrongPassword, 
            NewPassword = newPassword 
        };
        
        var user = new User(
            new Email("test@example.com"), 
            new Password(currentPassword)
        );
        user.Id = userId;
        
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        _mockRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _service.ChangePassword(dto));
        
        Assert.Equal("Senha inválida.", exception.Message);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_WithNewPasswordSameAsCurrent_ThrowsValidationException()
    {
        // Arrange
        var currentPassword = "Pass@123";
        var userId = 1;
        
        var dto = new ChangePasswordDto 
        { 
            Password = currentPassword, 
            NewPassword = currentPassword 
        };
        
        var user = new User(
            new Email("test@example.com"), 
            new Password(currentPassword)
        );
        user.Id = userId;
        
        _mockCurrentUser.Setup(c => c.UserId).Returns(userId);
        _mockRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _service.ChangePassword(dto));
        
        Assert.Equal("A nova senha não pode ser igual a senha atual.", exception.Message);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
}
