// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           IUnitOfWork.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Application-layer abstraction over the SQL transaction and outbox write cycle.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Application-layer abstraction over the SQL transaction + outbox cycle.
/// The UoW opens a transaction, executes all persistence, writes domain events
/// to OutboxMessages, then commits atomically.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>Active connection for the current unit of work.</summary>
    IDbConnection Connection { get; }

    /// <summary>Active transaction. All stored-procedure calls must use this.</summary>
    IDbTransaction Transaction { get; }

    /// <summary>Enqueues a domain event for outbox writing on commit.</summary>
    void EnqueueEvent(IDomainEvent domainEvent, string tenantId, string actorId, string correlationId);

    /// <summary>
    /// Writes all pending domain events to <c>OutboxMessages</c>, then commits
    /// the SQL transaction atomically.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>Rolls back the transaction and discards all pending events.</summary>
    Task RollbackAsync();
}
