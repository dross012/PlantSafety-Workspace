// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Continuous Monitoring & Review read-model.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.ContinuousMonitoringReview.Models;

namespace PlantSafety.Application.RiskManagement.ContinuousMonitoringReview.Repositories;

/// <summary>Read-only repository for Continuous Monitoring & Review data. Implemented in the Infrastructure layer.</summary>
public interface IContinuousMonitoringReviewRepository
{
    /// <summary>Returns Continuous Monitoring & Review items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ContinuousMonitoringReviewItemDto>> GetContinuousMonitoringReviewAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}