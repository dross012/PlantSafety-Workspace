// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           TransactionalUnitOfWork.cs
//  Created:        2026-07-21 (Claude Code -- platform-wide outbox bug fix)
//  Author:         PlantSafety Platform Team (Claude Code)
//  SchemaVersion:  2026.1
//  Description:    Infrastructure implementation of PlantSafety.Application.Behaviors.Transactions.
//                  IUnitOfWork -- the interface actually consumed by TransactionBehavior<,> for
//                  EVERY ICommand. Prior to this fix, this interface had NO DI registration:
//                  InfrastructureServiceRegistration only registered the differently-shaped
//                  PlantSafety.Infrastructure.Persistence.UnitOfWork.IUnitOfWork (Connection/
//                  Transaction/EnqueueEvent), which TransactionBehavior does not depend on. That
//                  meant TransactionBehavior<TRequest,TResponse> could never be constructed for
//                  any command, in any module -- i.e. no command could execute through the real
//                  MediatR pipeline. This class closes that gap.
//
//  KNOWN LIMITATION (by design, out of scope for this fix): every repository in this codebase
//  (old and new modules alike) opens and commits its OWN private IDbConnection/IDbTransaction per
//  Add/Update call (see e.g. RiskDapperRepository.AddAsync: "using var conn = ...; using var tx =
//  ...; ...; tx.Commit();"). That means the aggregate write and the outbox write are NOT in the
//  same SQL transaction -- true atomicity between them is not achievable without refactoring every
//  repository in the platform, which is explicitly out of scope here. Instead, CommitAsync below
//  flushes the outbox at the MediatR pipeline boundary: after the command handler (and therefore
//  every repository call within it) has already returned successfully, and before the response is
//  handed back to the caller. This preserves an eventual-consistency window (a process crash
//  between the aggregate's own SQL commit and this flush would lose the outbox write) that
//  pre-existed this fix -- it is not made worse by this change, just not solved by it.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// Default implementation of <see cref="PlantSafety.Application.Behaviors.Transactions.IUnitOfWork"/>.
/// Delegates outbox flushing to the scoped <see cref="IOutboxWriter"/> so that domain events raised
/// by repositories during handler execution (see the 17 legacy-taxonomy modules' *DapperRepository
/// AddAsync methods) are written to <c>[System].[OutboxMessages]</c> after a successful command.
/// See file-level remarks for the cross-transaction atomicity limitation this does NOT solve.
/// </summary>
public sealed class TransactionalUnitOfWork(
    IOutboxWriter outboxWriter,
    ILogger<TransactionalUnitOfWork> logger) : PlantSafety.Application.Behaviors.Transactions.IUnitOfWork
{
    /// <inheritdoc/>
    public bool IsTransactionActive { get; private set; }

    /// <inheritdoc/>
    public Task BeginAsync(CancellationToken cancellationToken = default)
    {
        IsTransactionActive = true;
        logger.LogDebug("TransactionalUnitOfWork: begin (logical -- see class remarks; no shared SQL transaction is opened here).");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await outboxWriter.FlushAsync(cancellationToken);
        IsTransactionActive = false;
        logger.LogDebug("TransactionalUnitOfWork: committed -- pending outbox events flushed to [System].[OutboxMessages].");
    }

    /// <inheritdoc/>
    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        outboxWriter.Clear();
        IsTransactionActive = false;
        logger.LogWarning("TransactionalUnitOfWork: rolled back -- pending outbox events discarded, no integration events published.");
        return Task.CompletedTask;
    }
}
