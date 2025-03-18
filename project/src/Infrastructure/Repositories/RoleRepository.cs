using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<Role> _dbSet;

    public RoleRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<Role>();
    }

    public async Task<Role?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task AddAsync(Role role)
    {
        await _dbSet.AddAsync(role);
    }

    public Task UpdateAsync(Role role)
    {
        _dbSet.Update(role);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var role = await GetByIdAsync(id);
        if (role != null)
        {
            _dbSet.Remove(role);
        }
    }

    public IQueryable<Role> Query()
    {
        return _dbSet.AsQueryable();
    }
}
