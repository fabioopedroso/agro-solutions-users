using Core.Entities;
using Core.Interfaces.Repositories;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repository;
public class UserRepository : IUserRepository
{
    protected ApplicationDbContext _context;
    protected DbSet<User> _dbSet;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<User>();
    }

    public async Task<User> AddAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        _dbSet.Add(user);

        if(_context.Database.CurrentTransaction != null)
        {
            await _context.SaveChangesAsync();
        }

        return user;
    }

    public async Task<User?> GetByEmailAsync(Email email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task UpdateAsync(User user)
    {
        _dbSet.Update(user);
        await _context.SaveChangesAsync();
    }
}
