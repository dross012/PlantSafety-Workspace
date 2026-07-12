// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single safety culture index snapshot.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Culture.CultureIndex.Models;

/// <summary>A single composite safety culture index snapshot for a given culture dimension.</summary>
public sealed record CultureIndexItemDto
{
    /// <summary>Index snapshot identifier.</summary>
    public required Guid IndexSnapshotId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Culture dimension measured (e.g., Trust, Communication, Accountability).</summary>
    public required string CultureDimension { get; init; }

    /// <summary>Composite index score for this dimension.</summary>
    public required decimal IndexScore { get; init; }

    /// <summary>Trend direction versus the prior period (e.g., Improving, Stable, Declining).</summary>
    public required string TrendDirection { get; init; }

    /// <summary>Current status of the snapshot (e.g., Draft, Published).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the index was calculated.</summary>
    public required DateTime CalculatedAtUtc { get; init; }
}
