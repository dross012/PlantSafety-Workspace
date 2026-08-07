// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Read-only Application-layer repository abstraction for the FMCSA standards
//   reference data (Compliance.FMCSAStandard/FMCSARequirement/FMCSAStandardChangeLog). Global
//   data -- every method is deliberately un-tenant-scoped, mirroring IOSHAStandardsRepository
//   exactly. Implemented in Infrastructure.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.FMCSAStandards.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Repositories;

/// <summary>Read-only repository for FMCSA standards reference data. Implemented in Infrastructure.</summary>
public interface IFmcsaStandardsRepository
{
    /// <summary>Returns a single standard by (PartNumber, SectionNumber), or null.</summary>
    Task<FmcsaStandardDto?> GetStandardBySectionAsync(string partNumber, string sectionNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Free-text/part search across Compliance.FMCSAStandard. <paramref name="keyword"/> matches
    /// against SectionTitle/SectionText (case-insensitive contains); all filters are optional and
    /// combine with AND. Results are ordered by PartNumber, SectionNumber.
    /// </summary>
    Task<IReadOnlyList<FmcsaStandardDto>> SearchStandardsAsync(
        string? keyword,
        string? partNumber,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>Returns every requirement derived from a given standard.</summary>
    Task<IReadOnlyList<FmcsaRequirementDto>> GetRequirementsForStandardAsync(Guid standardId, CancellationToken cancellationToken);

    /// <summary>Returns every requirement whose denormalized AppliesToModule column matches exactly.</summary>
    Task<IReadOnlyList<FmcsaRequirementDto>> GetRequirementsByModuleAsync(string appliesToModule, CancellationToken cancellationToken);

    /// <summary>Free-text search across Compliance.FMCSARequirement.RequirementText (case-insensitive contains).</summary>
    Task<IReadOnlyList<FmcsaRequirementDto>> SearchRequirementsAsync(string keyword, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Returns every change-log entry detected at or after <paramref name="sinceUtc"/>, most recent first.</summary>
    Task<IReadOnlyList<FmcsaStandardChangeDto>> GetChangesSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken);
}
