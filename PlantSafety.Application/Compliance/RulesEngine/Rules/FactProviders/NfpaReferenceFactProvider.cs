// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Gathers real NFPA hazard-profile facts for the Compliance Rules Engine by
//   calling the real, already-existing NFPAStandards queries (Compliance.NFPAStandards.Queries),
//   backed by Compliance.NFPA_HazardProfile/NFPA_ControlMatrix/NFPA_HazardModuleMapping/
//   NFPA_HazardChangeLog. No new query or schema introduced here -- this is the same read-model
//   tools/NfpaReferenceImport populates and PlantSafety.Api's NFPAStandardsV1Controller exposes
//   externally. Never throws; fails closed to null/empty on any lookup failure, mirroring
//   OshaStandardsFactProvider exactly. NFPA codes are voluntary consensus guidance, not
//   regulations -- callers should treat every fact here as "recommended", never "mandatory".
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Queries;
using PlantSafety.Application.Compliance.NFPAStandards.Models;
using PlantSafety.Application.Compliance.NFPAStandards.Queries;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.FactProviders;

/// <summary>Gathers real NFPA hazard-profile facts from the Compliance.NFPAStandards read-model.</summary>
public sealed class NfpaReferenceFactProvider(ISender sender)
{
    /// <summary>Returns the NFPA hazard profile for (CodeNumber, SectionNumber), or <see langword="null"/> when not found/failed.</summary>
    public async Task<NfpaHazardProfileDto?> GetHazardProfileAsync(string codeNumber, string sectionNumber, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNfpaHazardProfileQuery(codeNumber, sectionNumber), cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    /// <summary>Returns every NFPA hazard whose HazardName/HazardDescription contains <paramref name="keyword"/>, or an empty list on failure.</summary>
    public async Task<IReadOnlyList<NfpaHazardProfileDto>> SearchHazardsAsync(string keyword, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchNfpaHazardsQuery(keyword, CodeNumber: null, Skip: 0, Take: 500), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>Returns every recommended control for <paramref name="hazardId"/>, or an empty list on failure.</summary>
    public async Task<IReadOnlyList<NfpaControlMatrixDto>> GetControlsAsync(Guid hazardId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNfpaControlsForHazardQuery(hazardId), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>
    /// Returns every NFPA hazard mapped to <paramref name="moduleName"/>, or an empty list when
    /// none are mapped or the lookup failed. Fails closed/empty, never throws.
    /// </summary>
    public async Task<IReadOnlyList<NfpaHazardModuleDto>> GetModulesAsync(string moduleName, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNfpaHazardModulesQuery(moduleName), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>
    /// Returns per-module NFPA coverage exposure for a tenant/site, or an empty list on failure.
    /// Matches GetNfpaExposureForSiteQuery's own return shape (a per-module summary list, not a
    /// single snapshot), so callers get the same shape whether they call the query directly or
    /// through this fact provider.
    /// </summary>
    public async Task<IReadOnlyList<NfpaGapSummaryDto>> GetExposureAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNfpaExposureForSiteQuery(tenantId, siteId), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>Returns every NFPA hazard profile changed (new/revised/rescinded) at or after <paramref name="sinceUtc"/>, or an empty list on failure.</summary>
    public async Task<IReadOnlyList<NfpaHazardChangeDto>> GetChangesSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNfpaChangesSinceQuery(sinceUtc), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>
    /// Counts High/Critical-severity hazard mappings for <paramref name="moduleName"/> -- the
    /// fact behind "this job touches High-severity NFPA hazard guidance". Fails closed to zero on
    /// lookup failure -- an inability to check exposure is not evidence of low exposure.
    /// </summary>
    public async Task<NfpaHighSeverityExposure> GetHighSeverityNfpaExposureAsync(string moduleName, CancellationToken cancellationToken)
    {
        var hazards = await GetModulesAsync(moduleName, cancellationToken);

        var highCount = hazards.Count(h => h.Severity == "High");
        var criticalCount = hazards.Count(h => h.Severity == "Critical");

        return new NfpaHighSeverityExposure(moduleName, highCount, criticalCount);
    }
}

/// <summary>High/Critical-severity NFPA hazard-mapping counts for a single PlantSafety module.</summary>
/// <param name="Module">The module these counts apply to.</param>
/// <param name="HighCount">Number of mapped hazards rated High.</param>
/// <param name="CriticalCount">Number of mapped hazards rated Critical.</param>
public sealed record NfpaHighSeverityExposure(string Module, int HighCount, int CriticalCount)
{
    /// <summary>True when there is at least one High or Critical hazard mapping for this module.</summary>
    public bool HasExposure => HighCount > 0 || CriticalCount > 0;
}
