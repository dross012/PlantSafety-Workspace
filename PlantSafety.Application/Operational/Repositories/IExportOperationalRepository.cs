// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Repository abstraction for generating Operational data exports.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Queries;

namespace PlantSafety.Application.Operational.Repositories;

/// <summary>Generates Operational export files and uploads them to durable storage. Implemented in the Infrastructure layer.</summary>
public interface IExportOperationalRepository
{
    /// <summary>Generates an export file for the given filters and returns its download metadata.</summary>
    Task<ExportOperationalResult> GenerateExportAsync(
        Guid tenantId,
        Guid? siteId,
        int format,
        DateTime? from,
        DateTime to,
        string? status,
        int maxRecords,
        CancellationToken cancellationToken);
}
