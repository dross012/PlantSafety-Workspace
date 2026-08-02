// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-26
// FILE DESCRIPTION: Write-side repository abstraction for the Culture aggregate. Implemented in the Infrastructure layer.
// SchemaVersion: 2
// UPDATED: 2026-08-02 -- Phase 4 full build-out: added GetByIdAsync (tenant-scoped) and UpdateAsync
//   (optimistic-concurrency ETag), mirroring
//   PlantSafety.Domain.Compliance.Permits.Interfaces.IPermitsRepository's identical Phase 4
//   retrofit -- this aggregate previously had a Create-only write path (AddAsync), with no way to
//   load or mutate an existing Culture aggregate for the new Review/Publish/Close/Archive
//   lifecycle transitions.

using PlantSafety.Domain.Analytics.Culture.Aggregates;
using PlantSafety.Domain.Analytics.Culture.ValueObjects.Ids;

namespace PlantSafety.Application.Common.Interfaces.Persistence;

/// <summary>Write-side repository contract for the <see cref="Culture"/> aggregate.</summary>
public interface ICultureRepository
{
    /// <summary>Persists a newly created <see cref="Culture"/> aggregate.</summary>
    Task AddAsync(Culture culture, CancellationToken cancellationToken);

    /// <summary>Retrieves a Culture aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<Culture?> GetByIdAsync(CultureId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing Culture aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="culture">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(Culture culture, string? eTag, CancellationToken cancellationToken = default);
}
