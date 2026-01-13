using Core.Entities;
using Core.ValueObjects;

namespace Core.Interfaces.Repositories;
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(Email email);
    Task AddAsync(User user);
}
