// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the JobSafetyAnalysis read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.JobSafetyAnalysis.Models;

namespace PlantSafety.Application.Operational.JobSafetyAnalysis.Repositories;

/// <summary>Read-only repository for JobSafetyAnalysis data. Implemented in the Infrastructure layer.</summary>
public interface IJobSafetyAnalysisRepository
{
    /// <summary>Returns JobSafetyAnalysis items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<JobSafetyAnalysisItemDto>> GetJobSafetyAnalysisAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
