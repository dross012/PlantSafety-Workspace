// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO representing a scheduled/automated data export run pushed to an external integrated system (distinct from per-module ad-hoc export-download features elsewhere in this solution).
// SchemaVersion: 1

namespace PlantSafety.Application.Integrations.Export.Models;

/// <summary>Represents a scheduled/automated data export run pushed to an external integrated system (distinct from per-module ad-hoc export-download features elsewhere in this solution).</summary>
public sealed record ExportItemDto
{
    /// <summary>Identifier of the scheduled export run.</summary>
    public required Guid ExportRunId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>External system the data was exported to.</summary>
    public required string TargetSystem { get; init; }

    /// <summary>Category of data exported (e.g., Incidents, ComplianceRecords, FullDataset).</summary>
    public required string ExportType { get; init; }

    /// <summary>Number of records included in the export.</summary>
    public required int RecordCount { get; init; }

    /// <summary>Current export run status (e.g., Queued/Running/Completed/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the export run completed.</summary>
    public required DateTime CompletedAtUtc { get; init; }

    /// <summary>Identity of the user or service principal that initiated the export.</summary>
    public required Guid InitiatedBy { get; init; }
}
