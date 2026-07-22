// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Pre-Startup Safety Review (PSSR) read-model.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.PreStartupSafetyReview.Models;

namespace PlantSafety.Application.RiskManagement.PreStartupSafetyReview.Repositories;

/// <summary>Read-only repository for Pre-Startup Safety Review (PSSR) data. Implemented in the Infrastructure layer.</summary>
public interface IPreStartupSafetyReviewRepository
{
    /// <summary>Returns Pre-Startup Safety Review (PSSR) items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PreStartupSafetyReviewItemDto>> GetPreStartupSafetyReviewAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}