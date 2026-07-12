// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Repository abstraction for generating Compliance data exports.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Queries;

namespace PlantSafety.Application.Compliance.Repositories;

/// <summary>Generates Compliance export files and uploads them to durable storage. Implemented in the Infrastructure layer.</summary>
public interface IExportComplianceRepository
{
    /// <summary>Generates an export file for the given filters and returns its download metadata.</summary>
    Task<ExportComplianceResult> GenerateExportAsync(
        Guid tenantId,
        Guid? siteId,
        int format,
        DateTime? from,
        DateTime to,
        string? status,
        string? resourceType,
        int maxRecords,
        CancellationToken cancellationToken);
}
