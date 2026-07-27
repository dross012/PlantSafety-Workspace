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
    private readonly Messaging.CrossModule.ICrossModuleEventRelay? _crossModuleEventRelay;
    private readonly ILogger<UnitOfWork> _logger;
    private bool _disposed;

    /// <inheritdoc/>
    public IDbConnection Connection { get; }

    /// <inheritdoc/>
    public IDbTransaction Transaction { get; }

    /// <summary>Initialises the UoW, opens the connection, and begins a SQL transaction.</summary>
    /// <param name="crossModuleEventRelay">
    /// Optional. Relays the curated subset of domain events with a registered
    /// Shared.DomainEvents.Abstractions contract onto Shared.EventBus, kept entirely separate
    /// from the intra-module Outbox/ServiceBus pipeline above. Nullable so existing callers/
    /// tests that construct UnitOfWork without DI continue to compile unchanged.
    /// </param>
    public UnitOfWork(
        ISqlConnectionFactory factory,
        IOutboxWriter outboxWriter,
        ILogger<UnitOfWork> logger,
        Messaging.CrossModule.ICrossModuleEventRelay? crossModuleEventRelay = null)
    {
        _outboxWriter = outboxWriter;
        _crossModuleEventRelay = crossModuleEventRelay;
        _logger       = logger;
        Connection    = factory.CreateConnection();
        Transaction   = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
    }

    /// <inheritdoc/>
    public void EnqueueEvent(IDomainEvent domainEvent, string tenantId, string actorId, string correlationId)
    {
        _outboxWriter.Enqueue(domainEvent, tenantId, actorId, correlationId);
        _crossModuleEventRelay?.Enqueue(domainEvent, tenantId, actorId, correlationId);
    }

    /// <inheritdoc/>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _outboxWriter.FlushAsync(cancellationToken);
        Transaction.Commit();
        _logger.LogDebug("UnitOfWork committed successfully.");

        // Cross-module events are flushed AFTER the SQL transaction commits -- true
        // Publish-After-Commit semantics (patterns.md §6) for the Shared.EventBus sink,
        // distinct from the pre-commit OutboxMessages write above (existing behaviour,
        // unchanged). A relay failure here must not roll back an already-committed
        // transaction; it is logged and left for the relay's own retry/DLQ handling upstream.
        if (_crossModuleEventRelay is { HasPendingEvents: true })
        {
            await _crossModuleEventRelay.FlushAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public Task RollbackAsync()
    {
        Transaction.Rollback();
        _crossModuleEventRelay?.Clear();
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
