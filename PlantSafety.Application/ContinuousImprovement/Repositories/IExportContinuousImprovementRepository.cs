// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Repository abstraction for generating ContinuousImprovement data exports.
// SchemaVersion: 1

using PlantSafety.Application.ContinuousImprovement.Queries;

namespace PlantSafety.Application.ContinuousImprovement.Repositories;

/// <summary>Generates ContinuousImprovement export files and uploads them to durable storage. Implemented in the Infrastructure layer.</summary>
public interface IExportContinuousImprovementRepository
{
    /// <summary>Generates an export file for the given filters and returns its download metadata.</summary>
    Task<ExportContinuousImprovementResult> GenerateExportAsync(
        Guid tenantId,
        Guid? siteId,
        int format,
        DateTime? from,
        DateTime to,
        string? status,
        int maxRecords,
        CancellationToken cancellationToken);
}
