// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Repository abstraction for generating SafetyEvents data exports.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.Queries;

namespace PlantSafety.Application.SafetyEvents.Repositories;

/// <summary>Generates SafetyEvents export files and uploads them to durable storage. Implemented in the Infrastructure layer.</summary>
public interface IExportSafetyEventsRepository
{
    /// <summary>Generates an export file for the given filters and returns its download metadata.</summary>
    Task<ExportSafetyEventsResult> GenerateExportAsync(
        Guid tenantId,
        Guid? siteId,
        int format,
        DateTime? from,
        DateTime to,
        string? status,
        string? severity,
        int maxRecords,
        CancellationToken cancellationToken);
}
