// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Compliance read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.ScoreCards.Compliance.Models;

namespace PlantSafety.Application.Analytics.ScoreCards.Compliance.Repositories;

/// <summary>Read-only repository for Compliance data. Implemented in the Infrastructure layer.</summary>
public interface IComplianceRepository
{
    /// <summary>Returns Compliance items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ComplianceItemDto>> GetComplianceScoresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
