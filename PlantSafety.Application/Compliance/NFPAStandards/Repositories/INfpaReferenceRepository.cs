// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Read-only Application-layer repository abstraction for the NFPA hazard
//   profile reference data (Compliance.NFPA_HazardProfile/NFPA_ControlMatrix/
//   NFPA_HazardModuleMapping/NFPA_HazardChangeLog). Global data -- every method is deliberately
//   un-tenant-scoped, mirroring IOSHAStandardsRepository. Implemented in Infrastructure.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.NFPAStandards.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.Repositories;

/// <summary>Read-only repository for NFPA hazard profile reference data. Implemented in Infrastructure.</summary>
public interface INfpaReferenceRepository
{
    /// <summary>Returns a single hazard profile by (CodeNumber, SectionNumber), or null.</summary>
    Task<NfpaHazardProfileDto?> GetHazardProfileAsync(string codeNumber, string sectionNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Free-text/code search across Compliance.NFPA_HazardProfile. <paramref name="keyword"/>
    /// matches against HazardName/HazardDescription (case-insensitive contains); both filters are
    /// optional and combine with AND. Results are ordered by CodeNumber, SectionNumber.
    /// </summary>
    Task<IReadOnlyList<NfpaHazardProfileDto>> SearchHazardsAsync(
        string? keyword,
        string? codeNumber,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>Returns every recommended control derived for a given hazard.</summary>
    Task<IReadOnlyList<NfpaControlMatrixDto>> GetControlsForHazardAsync(Guid hazardId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns every hazard mapped to a given PlantSafety module, joined with its mapping row
    /// (see <see cref="NfpaHazardModuleDto"/> for the flattening rationale).
    /// </summary>
    Task<IReadOnlyList<NfpaHazardModuleDto>> GetHazardsByModuleAsync(string moduleName, CancellationToken cancellationToken);

    /// <summary>Returns every change-log entry detected at or after <paramref name="sinceUtc"/>, most recent first.</summary>
    Task<IReadOnlyList<NfpaHazardChangeDto>> GetChangesSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken);
}
