using Domain.Models;

namespace Application.Interfaces;

/// <summary>
/// Unit of Work interface to encapsulate repositories and transaction management.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }

    Task<Result<object>> BeginTransactionAsync();
    Task<Result<object>> CommitAsync();
    Task<Result<object>> RollbackAsync();
}
