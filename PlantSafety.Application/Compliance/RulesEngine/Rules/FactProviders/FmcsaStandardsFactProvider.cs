// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Gathers real FMCSA (49 CFR) regulatory facts for the Compliance Rules Engine
//   by calling the real, already-existing FMCSAStandards queries
//   (Compliance.FMCSAStandards.Queries), backed by
//   Compliance.FMCSAStandard/FMCSARequirement/FMCSAStandardChangeLog. No new query or schema
//   introduced here. FMCSA is a federal agency and 49 CFR is legally binding regulation, the
//   same legal character as OSHA's 29 CFR -- mirrors OshaStandardsFactProvider exactly.
//
//   GetRequirementsForModuleAsync only returns rows once a separate mapping pass has populated
//   Compliance.FMCSARequirement.AppliesToModule. Until that mapping exists this method returns
//   an empty list, which cross-domain rules should treat as "no known applicable requirements",
//   not as evidence of compliance. Never throws -- fails closed to null/empty on failure.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Queries;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.Queries;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.FactProviders;

/// <summary>Gathers real FMCSA regulatory facts from the Compliance.FMCSAStandards read-model.</summary>
public sealed class FmcsaStandardsFactProvider(ISender sender)
{
    /// <summary>Returns every FMCSA requirement derived from <paramref name="standardId"/>, or an empty list on failure.</summary>
    public async Task<IReadOnlyList<FmcsaRequirementDto>> GetRequirementsAsync(Guid standardId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFmcsaRequirementQuery(standardId), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>Returns every FMCSA requirement whose source text contains <paramref name="keyword"/>, or an empty list on failure.</summary>
    public async Task<IReadOnlyList<FmcsaRequirementDto>> SearchRequirementsAsync(string keyword, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchFmcsaRequirementsQuery(keyword, Skip: 0, Take: 500), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>
    /// Returns every FMCSA requirement mapped to <paramref name="appliesToModule"/>, or an empty
    /// list when none are mapped yet or the lookup failed. Fails closed/empty, never throws.
    /// </summary>
    public async Task<IReadOnlyList<FmcsaRequirementDto>> GetRequirementsForModuleAsync(string appliesToModule, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFmcsaRequirementsForModuleQuery(appliesToModule), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>Returns per-module FMCSA coverage exposure for a site, or an empty list on failure.</summary>
    public async Task<IReadOnlyList<FmcsaGapSummaryDto>> GetExposureAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        if (siteId is null) return [];

        var result = await sender.Send(new GetFmcsaExposureForSiteQuery(tenantId, siteId.Value), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>Returns every FMCSA standard changed (new/revised/rescinded) at or after <paramref name="sinceUtc"/>, or an empty list on failure.</summary>
    public async Task<IReadOnlyList<FmcsaStandardChangeDto>> GetChangesSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFmcsaChangesSinceQuery(sinceUtc), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>
    /// Counts High/Critical-severity requirements mapped to <paramref name="moduleName"/> -- the
    /// fact behind "this job touches High-severity FMCSA requirements". Fails closed to zero on
    /// lookup failure -- an inability to check exposure is not evidence of low exposure.
    /// </summary>
    public async Task<FmcsaHighSeverityExposure> GetHighSeverityFmcsaExposureAsync(string moduleName, CancellationToken cancellationToken)
    {
        var requirements = await GetRequirementsForModuleAsync(moduleName, cancellationToken);

        var highCount = requirements.Count(r => r.Severity == "High");
        var criticalCount = requirements.Count(r => r.Severity == "Critical");

        return new FmcsaHighSeverityExposure(moduleName, highCount, criticalCount);
    }
}

/// <summary>High/Critical-severity FMCSA requirement counts for a single PlantSafety module.</summary>
/// <param name="Module">The module these counts apply to.</param>
/// <param name="HighCount">Number of mapped requirements rated High.</param>
/// <param name="CriticalCount">Number of mapped requirements rated Critical.</param>
public sealed record FmcsaHighSeverityExposure(string Module, int HighCount, int CriticalCount)
{
    /// <summary>True when there is at least one High or Critical requirement mapped to this module.</summary>
    public bool HasExposure => HighCount > 0 || CriticalCount > 0;
}
