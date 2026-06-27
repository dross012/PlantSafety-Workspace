// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           UnitOfWork.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default IUnitOfWork that opens a SQL transaction and manages the outbox write cycle.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Default <see cref="IUnitOfWork"/> implementation. Opens a SQL transaction and
/// manages the canonical outbox write cycle (domain events -> OutboxMessages -> commit).
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly ILogger<UnitOfWork> _logger;
    private bool _disposed;

    /// <inheritdoc/>
    public IDbConnection Connection { get; }

    /// <inheritdoc/>
    public IDbTransaction Transaction { get; }

    /// <summary>Initialises the UoW, opens the connection, and begins a SQL transaction.</summary>
    public UnitOfWork(ISqlConnectionFactory factory, IOutboxWriter outboxWriter, ILogger<UnitOfWork> logger)
    {
        _outboxWriter = outboxWriter;
        _logger       = logger;
        Connection    = factory.CreateConnection();
        Transaction   = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
    }

    /// <inheritdoc/>
    public void EnqueueEvent(IDomainEvent domainEvent, string tenantId, string actorId, string correlationId)
        => _outboxWriter.Enqueue(domainEvent, tenantId, actorId, correlationId);

    /// <inheritdoc/>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _outboxWriter.FlushAsync(cancellationToken);
        Transaction.Commit();
        _logger.LogDebug("UnitOfWork committed successfully.");
    }

    /// <inheritdoc/>
    public Task RollbackAsync()
    {
        Transaction.Rollback();
        _logger.LogWarning("UnitOfWork rolled back.");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        Transaction.Dispose();
        Connection.Dispose();
        await ValueTask.CompletedTask;
    }
}
