// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Gathers real OSHA regulatory facts for the Compliance Rules Engine by
//   calling the real, already-existing OSHAStandards queries (Compliance.OSHAStandards.Queries),
//   backed by Compliance.OSHAStandard/OSHARequirement/OSHAStandardChangeLog. No new query or
//   schema introduced here -- this is the same read-model tools/OshaStandardsImport populates
//   and PlantSafety.Api's OSHAStandardsV1Controller exposes externally.
//
//   GetRequirementsAppliesToModuleAsync only returns rows once a separate mapping pass has
//   populated Compliance.OSHARequirement.AppliesToModule -- the import pipeline always leaves it
//   NULL (see OSHARequirement.sql/tools/OshaStandardsImport). Until that mapping exists this
//   method returns an empty list, which cross-domain rules should treat as "no known applicable
//   requirements", not as evidence of compliance.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Compliance.OSHAStandards.Models;
using PlantSafety.Application.Compliance.OSHAStandards.Queries;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.FactProviders;

/// <summary>Gathers real OSHA regulatory facts from the Compliance.OSHAStandards read-model.</summary>
public sealed class OshaStandardsFactProvider(ISender sender)
{
    /// <summary>Returns the OSHA standard for (PartNumber, Section), or <see langword="null"/> when not found/failed.</summary>
    public async Task<OSHAStandardItemDto?> GetStandardAsync(int partNumber, string section, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOshaStandardBySectionQuery(partNumber, section), cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    /// <summary>
    /// Returns every OSHA requirement mapped to <paramref name="appliesToModule"/>, or an empty
    /// list when none are mapped yet or the lookup failed. Fails closed/empty, never throws.
    /// </summary>
    public async Task<IReadOnlyList<OSHARequirementItemDto>> GetRequirementsAppliesToModuleAsync(string appliesToModule, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOshaRequirementsByModuleQuery(appliesToModule), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>Returns every OSHA requirement whose source sentence contains <paramref name="keyword"/>, or an empty list on failure.</summary>
    public async Task<IReadOnlyList<OSHARequirementItemDto>> SearchRequirementsAsync(string keyword, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchOshaRequirementsQuery(keyword, Skip: 0, Take: 500), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>Returns every OSHA standard changed (new/revised/rescinded) at or after <paramref name="sinceUtc"/>, or an empty list on failure.</summary>
    public async Task<IReadOnlyList<OSHAStandardChangeDto>> GetChangesSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOshaStandardChangesSinceQuery(sinceUtc), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>
    /// Counts High/Critical-severity requirements mapped to <paramref name="appliesToModule"/> --
    /// the fact behind "this job touches High-severity OSHA requirements". Severity here is the
    /// deterministic RequirementType-based rating tools/OshaStandardsImport's Phase 6 mapping
    /// pass assigns (see Compliance.OSHARequirementModuleMapping's header), not a CFR-native
    /// value. Fails closed to zero on lookup failure -- an inability to check exposure is not
    /// evidence of low exposure.
    /// </summary>
    public async Task<OshaHighSeverityExposure> GetHighSeverityExposureAsync(string appliesToModule, CancellationToken cancellationToken)
    {
        var requirements = await GetRequirementsAppliesToModuleAsync(appliesToModule, cancellationToken);

        var highCount = requirements.Count(r => r.Severity == "High");
        var criticalCount = requirements.Count(r => r.Severity == "Critical");

        return new OshaHighSeverityExposure(appliesToModule, highCount, criticalCount);
    }
}

/// <summary>High/Critical-severity OSHA requirement counts for a single PlantSafety module.</summary>
/// <param name="Module">The module these counts apply to.</param>
/// <param name="HighCount">Number of mapped requirements rated High.</param>
/// <param name="CriticalCount">Number of mapped requirements rated Critical.</param>
public sealed record OshaHighSeverityExposure(string Module, int HighCount, int CriticalCount)
{
    /// <summary>True when there is at least one High or Critical requirement mapped to this module.</summary>
    public bool HasExposure => HighCount > 0 || CriticalCount > 0;
}
