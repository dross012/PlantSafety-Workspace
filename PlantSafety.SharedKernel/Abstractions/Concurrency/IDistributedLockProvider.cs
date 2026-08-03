/*==========================================================================================================================
    AUTHOR:             PlantSafety Platform Team (Claude Code)
    CREATED:            2026-08-02
    FILE:               IDistributedLockProvider.cs
    DESCRIPTION:        Canonical abstraction over a cross-process, cross-instance distributed lock. Provides
                        deterministic, SIEM-safe mutual exclusion over a caller-supplied resource key so that
                        concurrent read-modify-write cycles against the same aggregate (e.g. a single
                        WorkflowInstance mutated by two racing command handlers) serialize instead of
                        interleaving. This interface is infrastructure-agnostic and MUST NOT leak the backing
                        lock technology (SQL Server sp_getapplock, Redis, etc.) into callers.

    USAGE EXAMPLE (Application-layer command handler or MediatR pipeline behavior):
                        <code>
                        await using var handle = await lockProvider.AcquireAsync(
                            $"Workflow:Instance:{workflowInstanceId:D}", TimeSpan.FromSeconds(10), cancellationToken);
                        // ... load aggregate, apply transition, persist ...
                        // lock is released automatically when 'handle' is disposed.
                        </code>
                        A future cross-module consumer (see
                        PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration) protecting an
                        idempotent read-modify-write against a shared resource should follow the identical
                        pattern, keyed by that resource's own identity (e.g. "Maintenance:WorkOrder:{id}").

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Concurrency;

/// <summary>
/// Canonical abstraction over a cross-process, cross-instance distributed lock, keyed by an
/// arbitrary caller-supplied string resource identifier. MUST be deterministic, SIEM-safe, and
/// infrastructure-agnostic. Implementations MUST guarantee mutual exclusion across every process
/// and instance sharing the same backing store (e.g. the same SQL Server database), not merely
/// within a single process.
/// </summary>
public interface IDistributedLockProvider
{
    /// <summary>
    /// Acquires an exclusive distributed lock over <paramref name="resourceKey"/>, waiting up to
    /// <paramref name="timeout"/> for the lock to become available. Throws if the lock could not
    /// be acquired within the timeout; use <see cref="TryAcquireAsync"/> for a non-throwing
    /// variant.
    /// </summary>
    /// <param name="resourceKey">
    /// Caller-supplied resource identifier. Callers SHOULD namespace this key with the owning
    /// module/aggregate (e.g. <c>"Workflow:Instance:{workflowInstanceId}"</c>) to avoid
    /// cross-module collisions.
    /// </param>
    /// <param name="timeout">Maximum duration to wait for the lock before failing.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>
    /// An <see cref="IAsyncDisposable"/> lock handle. The lock is held for as long as the handle
    /// remains undisposed and is released deterministically when the handle is disposed
    /// (typically via <c>await using</c>).
    /// </returns>
    Task<IAsyncDisposable> AcquireAsync(string resourceKey, TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to acquire an exclusive distributed lock over <paramref name="resourceKey"/>,
    /// waiting up to <paramref name="timeout"/> for the lock to become available. Never throws
    /// solely because the lock could not be acquired within the timeout.
    /// </summary>
    /// <param name="resourceKey">
    /// Caller-supplied resource identifier. Callers SHOULD namespace this key with the owning
    /// module/aggregate (e.g. <c>"Workflow:Instance:{workflowInstanceId}"</c>) to avoid
    /// cross-module collisions.
    /// </param>
    /// <param name="timeout">Maximum duration to wait for the lock before giving up.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>
    /// An <see cref="IAsyncDisposable"/> lock handle if the lock was acquired within
    /// <paramref name="timeout"/>; otherwise <see langword="null"/>.
    /// </returns>
    Task<IAsyncDisposable?> TryAcquireAsync(string resourceKey, TimeSpan timeout, CancellationToken cancellationToken = default);
}
