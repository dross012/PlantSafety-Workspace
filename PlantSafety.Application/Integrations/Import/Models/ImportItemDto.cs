// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO representing a data import job run received from an external system (e.g. batch file imports).
// SchemaVersion: 1

namespace PlantSafety.Application.Integrations.Import.Models;

/// <summary>Represents a data import job run received from an external system (e.g. batch file imports).</summary>
public sealed record ImportItemDto
{
    /// <summary>Identifier of the import job run.</summary>
    public required Guid ImportRunId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>External system the data was imported from.</summary>
    public required string SourceSystem { get; init; }

    /// <summary>Category of data imported (e.g., Incidents, Employees, TrainingRecords).</summary>
    public required string ImportType { get; init; }

    /// <summary>Number of records successfully processed.</summary>
    public required int RecordsProcessed { get; init; }

    /// <summary>Number of records that failed validation or processing.</summary>
    public required int RecordsFailed { get; init; }

    /// <summary>Current import run status (e.g., Queued/Running/Completed/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the import run completed.</summary>
    public required DateTime CompletedAtUtc { get; init; }

    /// <summary>Identity of the user or service principal that initiated the import.</summary>
    public required Guid InitiatedBy { get; init; }
}
