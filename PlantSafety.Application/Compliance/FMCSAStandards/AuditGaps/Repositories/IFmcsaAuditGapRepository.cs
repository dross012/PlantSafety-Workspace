// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Read-only repository abstraction spanning Compliance.FMCSARequirement and
//   Auditing.AuditFindingFmcsaCoverage/AuditFinding -- the one legitimate cross-context read this
//   codebase supports for FMCSA gap detection. Mirrors IOshaAuditGapRepository exactly.
//   Implemented in Infrastructure.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Repositories;

/// <summary>Read-only repository for FMCSA requirement coverage gaps and exposure dashboards.</summary>
public interface IFmcsaAuditGapRepository
{
    /// <summary>
    /// Returns requirements mapped to <paramref name="moduleName"/> (or every mapped module when
    /// null) with no covering AuditFinding at the site.
    /// </summary>
    Task<IReadOnlyList<FmcsaGapDto>> GetGapsForSiteAsync(Guid tenantId, Guid? siteId, string? moduleName, CancellationToken cancellationToken);

    /// <summary>Returns aggregate coverage counts per module (or the single module when <paramref name="moduleName"/> is given) at the site.</summary>
    Task<IReadOnlyList<FmcsaGapSummaryDto>> GetGapsSummaryForSiteAsync(Guid tenantId, Guid? siteId, string? moduleName, CancellationToken cancellationToken);

    /// <summary>Returns total/High/Critical requirement counts for every module -- global, not tenant/site scoped.</summary>
    Task<IReadOnlyList<FmcsaModuleTotalDto>> GetModuleTotalsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns coverage counts for every module at the given site, and records a
    /// Compliance.FMCSAExposureSnapshot row per module as a side effect.
    /// </summary>
    Task<IReadOnlyList<FmcsaGapSummaryDto>> GetSiteExposureAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns coverage counts for every module across the whole tenant (any site's AuditFinding
    /// counts as coverage), and records a tenant-wide (SiteId NULL) snapshot row per module.
    /// </summary>
    Task<IReadOnlyList<FmcsaGapSummaryDto>> GetTenantExposureAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns the <paramref name="take"/> highest-severity uncovered requirements across every module at the site.</summary>
    Task<IReadOnlyList<FmcsaGapDto>> GetTopUncoveredForSiteAsync(Guid tenantId, Guid? siteId, int take, CancellationToken cancellationToken);

    /// <summary>
    /// Returns recorded exposure snapshots for <paramref name="module"/> between
    /// <paramref name="fromUtc"/> and <paramref name="toUtc"/>, oldest first.
    /// <paramref name="siteId"/> null means the tenant-wide snapshot series.
    /// </summary>
    Task<IReadOnlyList<FmcsaExposureSnapshotDto>> GetExposureTrendAsync(Guid tenantId, Guid? siteId, string module, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);
}
