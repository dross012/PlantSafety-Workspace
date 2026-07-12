// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO for a single data lake ingestion/curation activity record.
// SchemaVersion: 1
namespace PlantSafety.Application.Intelligence.DataLake.Models;

/// <summary>Represents a single data lake ingestion/curation activity record.</summary>
public sealed record DataLakeItemDto
{
    /// <summary>Unique identifier of the data lake activity record.</summary>
    public required Guid DataLakeRecordId { get; init; }

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

    /// <summary>Current activity status (e.g., Ingested/Processing/Curated/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the activity was processed.</summary>
    public required DateTime ProcessedAtUtc { get; init; }

    /// <summary>Identity of the user or process that performed the activity.</summary>
    public required Guid ProcessedBy { get; init; }
}