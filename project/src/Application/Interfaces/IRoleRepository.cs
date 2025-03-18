using Domain.Entities;

namespace Application.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id);
    Task AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(Guid id);
    IQueryable<Role> Query();
}
