// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Reporting & Distribution read-model.
// SchemaVersion: 1

using PlantSafety.Application.Scorecards.ReportingDistribution.Models;

namespace PlantSafety.Application.Scorecards.ReportingDistribution.Repositories;

/// <summary>Read-only repository for Reporting &amp; Distribution data. Implemented in the Infrastructure layer.</summary>
public interface IReportingDistributionRepository
{
    /// <summary>Returns Reporting &amp; Distribution items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ReportingDistributionItemDto>> GetReportingDistributionAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}