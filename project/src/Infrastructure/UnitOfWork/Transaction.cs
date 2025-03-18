using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Infrastructure.UnitOfWork;

public class Transaction : IAsyncDisposable, IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Transaction> _logger;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public Transaction(ApplicationDbContext dbContext, ILogger<Transaction> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<object>> BeginTransactionAsync()
    {
        if (_disposed)
        {
            return Result<object>.Failure("Cannot begin a transaction on a disposed object.");
        }

        if (_transaction != null)
        {
            return Result<object>.Failure("A transaction is already in progress.");
        }

        _transaction = await _dbContext.Database.BeginTransactionAsync();
        return Result<object>.Success("Transaction started successfully.");
    }

    public async Task<Result<object>> CommitAsync()
    {
        if (_transaction == null)
        {
            return Result<object>.Failure("No transaction in progress to commit.");
        }

        try
        {
            await _dbContext.SaveChangesAsync();
            await _transaction.CommitAsync();
            await DisposeTransactionAsync();
            return Result<object>.Success("Transaction committed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Commit operation failed.");
            return Result<object>.Failure($"Commit failed: {ex.Message}");
        }
    }

    public async Task<Result<object>> RollbackAsync()
    {
        if (_transaction == null)
        {
            return Result<object>.Failure("No transaction in progress to roll back.");
        }

        try
        {
            await _transaction.RollbackAsync();
            await DisposeTransactionAsync();
            return Result<object>.Success("Transaction rolled back successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rollback operation failed.");
            return Result<object>.Failure($"Rollback failed: {ex.Message}");
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _transaction?.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
