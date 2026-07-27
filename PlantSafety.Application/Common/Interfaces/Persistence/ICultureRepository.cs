// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-26
// FILE DESCRIPTION: Write-side repository abstraction for the Culture aggregate. Implemented in the Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Domain.Analytics.Culture.Aggregates;

namespace PlantSafety.Application.Common.Interfaces.Persistence;

/// <summary>Write-side repository contract for the <see cref="Culture"/> aggregate.</summary>
public interface ICultureRepository
{
    /// <summary>Persists a newly created <see cref="Culture"/> aggregate.</summary>
    Task AddAsync(Culture culture, CancellationToken cancellationToken);
}
