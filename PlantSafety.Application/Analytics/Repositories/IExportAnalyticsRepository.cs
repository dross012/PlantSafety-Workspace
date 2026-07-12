// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Repository abstraction for generating Analytics data exports.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Queries;

namespace PlantSafety.Application.Analytics.Repositories;

/// <summary>Generates Analytics export files and uploads them to durable storage. Implemented in the Infrastructure layer.</summary>
public interface IExportAnalyticsRepository
{
    /// <summary>Generates an export file for the given filters and returns its download metadata.</summary>
    Task<ExportAnalyticsResult> GenerateExportAsync(
        Guid tenantId,
        Guid? siteId,
        int format,
        DateTime? from,
        DateTime to,
        string? status,
        int maxRecords,
        CancellationToken cancellationToken);
}
