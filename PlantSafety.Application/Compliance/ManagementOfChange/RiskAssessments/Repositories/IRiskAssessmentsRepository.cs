// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the RiskAssessments read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.ManagementOfChange.RiskAssessments.Models;

namespace PlantSafety.Application.Compliance.ManagementOfChange.RiskAssessments.Repositories;

/// <summary>Read-only repository for RiskAssessments data. Implemented in the Infrastructure layer.</summary>
public interface IRiskAssessmentsRepository
{
    /// <summary>Returns RiskAssessments items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RiskAssessmentsItemDto>> GetRiskAssessmentsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
