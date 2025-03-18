using Application.Interfaces;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Infrastructure.UnitOfWork;

/// <summary>
/// Unit of Work implementation to manage repositories and transactions.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly Transaction _transaction;
    private readonly ILogger<UnitOfWork> _logger;
    private bool _disposed = false; // Track whether Dispose has been called

    public IUserRepository Users { get; init; }
    public IRoleRepository Roles { get; init; }

    public UnitOfWork(
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        IUserRepository users,
        IRoleRepository roles
    )
    {
        _transaction = new Transaction(dbContext, loggerFactory.CreateLogger<Transaction>());
        _logger = loggerFactory.CreateLogger<UnitOfWork>();
        Users = users;
        Roles = roles;
    }

    public async Task<Result<object>> BeginTransactionAsync()
    {
        var result = await _transaction.BeginTransactionAsync();
        if (!result.IsSuccess)
        {
            _logger.LogError("Failed to begin transaction: {Message}", result.Message);
        }
        return result;
    }

    public async Task<Result<object>> CommitAsync()
    {
        var result = await _transaction.CommitAsync();
        if (!result.IsSuccess)
        {
            _logger.LogError("Commit failed: {Message}", result.Message);
        }
        return result;
    }

    public async Task<Result<object>> RollbackAsync()
    {
        var result = await _transaction.RollbackAsync();
        if (!result.IsSuccess)
        {
            _logger.LogError("Rollback failed: {Message}", result.Message);
        }
        return result;
    }

    /// <summary>
    /// Dispose Pattern Implementation
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _transaction.Dispose();
            }

            // No unmanaged resources to release

            _disposed = true;
        }
    }

    /// <summary>
    /// Dispose Implementation
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this); // Suppress finalization
    }

    /// <summary>
    /// Asynchronous Dispose Implementation
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _transaction.DisposeAsync();
            Dispose(disposing: false);
        }
    }

    /// <summary>
    /// Finalizer
    /// </summary>
    ~UnitOfWork()
    {
        Dispose(disposing: false);
    }
}
