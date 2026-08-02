// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Application
//  File:           DataLakeActivityDetailDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single DataLakeActivity record, including every
//                  aggregate property plus an ETag. Backs GetDataLakeActivityByIdQuery. Distinct
//                  from the pre-existing, simpler DataLakeItemDto used by the date-range list query
//                  (GetDataLakeQuery), which is left entirely untouched by this pass.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Intelligence.DataLake.Models;

/// <summary>A single DataLakeActivity read-model record, including the full lifecycle state.</summary>
public sealed record DataLakeActivityDetailDto
{
    /// <summary>Data lake activity identifier.</summary>
    public required Guid DataLakeActivityId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the dataset associated with this record.</summary>
    public required string DatasetName { get; init; }

    /// <summary>Data lake zone the record resides in (e.g., Raw/Cleansed/Curated/ML/Sandbox).</summary>
    public required string Zone { get; init; }

    /// <summary>Number of records processed in this activity.</summary>
    public long? RecordCount { get; init; }

    /// <summary>Current lifecycle status (Ingested/Processing/Curated/Failed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the activity was processed (or first ingested).</summary>
    public required DateTime ProcessedAtUtc { get; init; }

    /// <summary>Identity of the user or process that performed the activity.</summary>
    public required Guid ProcessedBy { get; init; }

    /// <summary>Identity of the user who started processing (transitioned to Processing).</summary>
    public Guid? ProcessingStartedBy { get; init; }

    /// <summary>UTC instant the activity transitioned to Processing.</summary>
    public DateTime? ProcessingStartedAtUtc { get; init; }

    /// <summary>UTC instant the activity's data was curated.</summary>
    public DateTime? CuratedAtUtc { get; init; }

    /// <summary>Failure reason recorded when the activity failed, if any.</summary>
    public string? FailureReason { get; init; }

    /// <summary>UTC instant the activity failed.</summary>
    public DateTime? FailedAtUtc { get; init; }

    /// <summary>UTC instant the activity was archived for compliance retention.</summary>
    public DateTime? ArchivedAtUtc { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
