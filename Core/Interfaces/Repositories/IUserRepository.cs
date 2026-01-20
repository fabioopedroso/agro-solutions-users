using Core.Entities;
using Core.ValueObjects;

namespace Core.Interfaces.Repositories;
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(Email email);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
}
