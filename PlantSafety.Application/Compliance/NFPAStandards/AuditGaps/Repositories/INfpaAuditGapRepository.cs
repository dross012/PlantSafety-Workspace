// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Read-only repository abstraction spanning Compliance.NFPA_HazardModuleMapping
//   and Auditing.AuditFindingNfpaCoverage/AuditFinding -- the one legitimate cross-context read
//   this codebase supports for NFPA gap detection (see AuditFindingNfpaCoverage.sql). Implemented
//   in Infrastructure. Tenant-scoped (unlike INfpaReferenceRepository), matching OSHA's split
//   between IOSHAStandardsRepository (global) and IOshaAuditGapRepository (tenant-scoped).
// SchemaVersion: 1

using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Repositories;

/// <summary>Read-only repository for NFPA hazard mapping coverage gaps and exposure dashboards.</summary>
public interface INfpaAuditGapRepository
{
    /// <summary>Returns every hazard mapping with no covering AuditFinding at the site, optionally filtered to <paramref name="moduleName"/>.</summary>
    Task<IReadOnlyList<NfpaGapDto>> GetGapsForSiteAsync(Guid tenantId, Guid? siteId, string? moduleName, CancellationToken cancellationToken);

    /// <summary>Returns aggregate coverage counts per module at the site, optionally filtered to <paramref name="moduleName"/>.</summary>
    Task<IReadOnlyList<NfpaGapSummaryDto>> GetGapsSummaryForSiteAsync(Guid tenantId, Guid? siteId, string? moduleName, CancellationToken cancellationToken);

    /// <summary>Returns total/High/Critical hazard-mapping counts for every module -- global, not tenant/site scoped.</summary>
    Task<IReadOnlyList<NfpaModuleTotalDto>> GetModuleTotalsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns coverage counts for every module at the given site, and records a
    /// Compliance.NFPAExposureSnapshot row per module as a side effect (this is how exposure
    /// trend history accumulates).
    /// </summary>
    Task<IReadOnlyList<NfpaGapSummaryDto>> GetSiteExposureAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns coverage counts for every module across the whole tenant (any site's AuditFinding
    /// counts as coverage), and records a tenant-wide (SiteId NULL) snapshot row per module.
    /// </summary>
    Task<IReadOnlyList<NfpaGapSummaryDto>> GetTenantExposureAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns the <paramref name="take"/> highest-severity uncovered hazard mappings across every module at the site.</summary>
    Task<IReadOnlyList<NfpaGapDto>> GetTopUncoveredForSiteAsync(Guid tenantId, Guid? siteId, int take, CancellationToken cancellationToken);

    /// <summary>
    /// Returns recorded exposure snapshots for <paramref name="module"/> between
    /// <paramref name="fromUtc"/> and <paramref name="toUtc"/>, oldest first.
    /// <paramref name="siteId"/> null means the tenant-wide snapshot series.
    /// </summary>
    Task<IReadOnlyList<NfpaExposureSnapshotDto>> GetExposureTrendAsync(Guid tenantId, Guid? siteId, string module, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);
}
