// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Read-only Application-layer repository abstraction for the NIOSH
//   reference data (Compliance.NIOSHHazardProfile/NIOSHRespiratorMatrix/NIOSHPPEGuidance/
//   NIOSHHazardModuleMapping/NIOSHHazardChangeLog). Global data -- every method is
//   deliberately un-tenant-scoped, matching OSHAStandards' precedent for reference data.
//   This is REFERENCE DATA (chemical hazard profiles, respirator selection, PPE guidance),
//   not a regulatory requirement. Implemented in Infrastructure.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.NIOSHStandards.Models;

namespace PlantSafety.Application.Compliance.NIOSHStandards.Repositories;

/// <summary>Read-only repository for NIOSH reference data. Implemented in Infrastructure.</summary>
public interface INioshReferenceRepository
{
    /// <summary>Returns a single hazard profile by CAS Registry Number, or null.</summary>
    Task<NioshHazardProfileDto?> GetHazardProfileByCasAsync(string casNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Free-text search across Compliance.NIOSHHazardProfile. <paramref name="keyword"/> matches
    /// against ChemicalName/CASNumber (case-insensitive contains). Results are paged.
    /// </summary>
    Task<IReadOnlyList<NioshHazardProfileDto>> SearchHazardsAsync(string? keyword, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Returns every respirator recommendation for a given hazard profile.</summary>
    Task<IReadOnlyList<NioshRespiratorMatrixDto>> GetRespiratorRecommendationsAsync(Guid hazardId, CancellationToken cancellationToken);

    /// <summary>Returns every PPE guidance entry for a given hazard profile.</summary>
    Task<IReadOnlyList<NioshPpeGuidanceDto>> GetPpeGuidanceAsync(Guid hazardId, CancellationToken cancellationToken);

    /// <summary>Returns every hazard profile mapped to a given PlantSafety module (Compliance.NIOSHHazardModuleMapping.Module exact match).</summary>
    Task<IReadOnlyList<NioshHazardProfileDto>> GetHazardsByModuleAsync(string module, CancellationToken cancellationToken);

    /// <summary>Returns every change-log entry detected at or after <paramref name="sinceUtc"/>, most recent first.</summary>
    Task<IReadOnlyList<NioshHazardChangeDto>> GetChangesSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken);
}
