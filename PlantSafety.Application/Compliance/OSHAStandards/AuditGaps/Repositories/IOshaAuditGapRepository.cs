// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Read-only repository abstraction spanning Compliance.OSHARequirement and
//   Auditing.AuditFindingOshaCoverage/AuditFinding -- the one legitimate cross-context read this
//   codebase supports for OSHA gap detection (see AuditFindingOshaCoverage.sql). Implemented in
//   Infrastructure.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Models;

namespace PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Repositories;

/// <summary>Read-only repository for OSHA requirement coverage gaps and exposure dashboards.</summary>
public interface IOshaAuditGapRepository
{
    /// <summary>Returns every requirement mapped to <paramref name="module"/> with no covering AuditFinding at the site.</summary>
    Task<IReadOnlyList<OshaGapDto>> GetGapsForSiteAsync(Guid tenantId, Guid siteId, string module, CancellationToken cancellationToken);

    /// <summary>Returns aggregate coverage counts for <paramref name="module"/> at the site.</summary>
    Task<OshaGapSummaryDto> GetGapsSummaryForSiteAsync(Guid tenantId, Guid siteId, string module, CancellationToken cancellationToken);

    /// <summary>Returns total/High/Critical requirement counts for every module -- global, not tenant/site scoped.</summary>
    Task<IReadOnlyList<OshaModuleTotalDto>> GetModuleTotalsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns coverage counts for every module at the given site, and records a
    /// Compliance.OSHAExposureSnapshot row per module as a side effect (this is how exposure
    /// trend history accumulates -- see OSHAExposureSnapshot.sql's header).
    /// </summary>
    Task<IReadOnlyList<OshaGapSummaryDto>> GetSiteExposureAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns coverage counts for every module across the whole tenant (any site's AuditFinding
    /// counts as coverage), and records a tenant-wide (SiteId NULL) snapshot row per module.
    /// </summary>
    Task<IReadOnlyList<OshaGapSummaryDto>> GetTenantExposureAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns the <paramref name="take"/> highest-severity uncovered requirements across every module at the site.</summary>
    Task<IReadOnlyList<OshaGapDto>> GetTopUncoveredForSiteAsync(Guid tenantId, Guid siteId, int take, CancellationToken cancellationToken);

    /// <summary>
    /// Returns recorded exposure snapshots for <paramref name="module"/> between
    /// <paramref name="fromUtc"/> and <paramref name="toUtc"/>, oldest first.
    /// <paramref name="siteId"/> null means the tenant-wide snapshot series.
    /// </summary>
    Task<IReadOnlyList<OshaExposureSnapshotDto>> GetExposureTrendAsync(Guid tenantId, Guid? siteId, string module, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);
}
