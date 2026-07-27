// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-26
// FILE DESCRIPTION: Application-layer outbox writer contract used by command handlers to flush
//   pending domain events after a successful Unit of Work commit. This interface deliberately
//   MIRRORS (does not share/reuse) PlantSafety.Infrastructure.Common.Pipelines.IOutboxWriter —
//   Application must never depend on Infrastructure (Clean Architecture dependency direction),
//   so both layers declare their own minimal copy of the same shape.
// SchemaVersion: 1

namespace PlantSafety.Application.Common.Interfaces.Persistence;

/// <summary>
/// Application-layer contract for writing pending domain events to the outbox.
/// Mirrors the Infrastructure-layer <c>IOutboxWriter</c> by design — see file header.
/// </summary>
public interface IOutboxWriter
{
    /// <summary>Returns <see langword="true"/> when there are un-flushed domain events.</summary>
    bool HasPendingEvents { get; }

    /// <summary>Writes all pending domain events to the outbox table.</summary>
    Task FlushAsync(CancellationToken cancellationToken = default);

    /// <summary>Discards all pending (un-flushed) domain events without writing them.</summary>
    void Clear();
}
