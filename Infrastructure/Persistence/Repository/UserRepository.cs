using Core.Entities;
using Core.Interfaces.Repositories;
using Core.ValueObjects;

namespace Infrastructure.Persistence.Repository;
public class UserRepository : IUserRepository
{
    public Task AddAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByEmailAsync(Email email)
    {
        throw new NotImplementedException();
    }
}
