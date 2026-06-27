// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Unit of Work abstraction that guarantees Publish-After-Commit semantics.
//   During command execution the UoW:
//     1. Opens a SQL transaction
//     2. Executes all stored procedures to persist aggregate state
//     3. Transforms domain events into canonical envelopes
//     4. Writes envelopes to OutboxMessages table
//     5. Commits atomically
//   If any step fails, transaction is rolled back, no integration events are published.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no existing members may be removed or renamed.

namespace PlantSafety.Application.Behaviors.Transactions;

/// <summary>
/// Canonical Unit of Work abstraction for atomic persistence and event envelope emission.
/// Infrastructure provides the SQL + Outbox implementation.
/// Application layer (via <see cref="TransactionBehavior{TRequest,TResponse}"/>) calls
/// <see cref="BeginAsync"/>, then <see cref="CommitAsync"/> on success or <see cref="RollbackAsync"/> on failure.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Opens a new SQL transaction scoped to the current command execution.</summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task BeginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the open transaction.
    /// Before committing: transforms pending domain events into canonical envelopes and writes to OutboxMessages.
    /// After committing: signals the Outbox Processor to publish integration events.
    /// </summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the open transaction.
    /// Discards all pending domain events; no integration events are published.
    /// </summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns <c>true</c> if a transaction is currently open.</summary>
    bool IsTransactionActive { get; }
}
