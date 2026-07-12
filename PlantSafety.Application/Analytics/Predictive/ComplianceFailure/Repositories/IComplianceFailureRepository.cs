// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the ComplianceFailure read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Predictive.ComplianceFailure.Models;

namespace PlantSafety.Application.Analytics.Predictive.ComplianceFailure.Repositories;

/// <summary>Read-only repository for ComplianceFailure data. Implemented in the Infrastructure layer.</summary>
public interface IComplianceFailureRepository
{
    /// <summary>Returns ComplianceFailure items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ComplianceFailureItemDto>> GetComplianceFailurePredictionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
