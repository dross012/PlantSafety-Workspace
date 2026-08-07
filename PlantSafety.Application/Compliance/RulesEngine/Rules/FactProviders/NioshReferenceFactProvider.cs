// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Gathers real NIOSH reference-data facts for the Compliance Rules Engine by
//   calling the real, already-existing NIOSHStandards queries (Compliance.NIOSHStandards.Queries),
//   backed by Compliance.NIOSHHazardProfile/NIOSHRespiratorMatrix/NIOSHPPEGuidance/
//   NIOSHHazardModuleMapping/NIOSHHazardChangeLog. No new query or schema introduced here -- this
//   is the same read-model PlantSafety.Api's NIOSHStandardsV1Controller exposes externally. This
//   is REFERENCE DATA (chemical hazard profiles, respirator selection, PPE guidance), not a
//   regulatory requirement, so no "requirements applies to module" concept exists here the way it
//   does for OshaStandardsFactProvider.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Compliance.NIOSHStandards.Models;
using PlantSafety.Application.Compliance.NIOSHStandards.Queries;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.FactProviders;

/// <summary>Gathers real NIOSH reference-data facts from the Compliance.NIOSHStandards read-model.</summary>
public sealed class NioshReferenceFactProvider(ISender sender)
{
    /// <summary>Returns the NIOSH hazard profile for a CAS Registry Number, or <see langword="null"/> when not found/failed.</summary>
    public async Task<NioshHazardProfileDto?> GetHazardProfileAsync(string casNumber, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNioshHazardProfileQuery(casNumber), cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    /// <summary>Returns every respirator recommendation for a hazard profile, or an empty list on failure. Fails closed/empty, never throws.</summary>
    public async Task<IReadOnlyList<NioshRespiratorMatrixDto>> GetRespiratorRecommendationsAsync(Guid hazardId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRespiratorRecommendationsQuery(hazardId), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>Returns every PPE guidance entry for a hazard profile, or an empty list on failure.</summary>
    public async Task<IReadOnlyList<NioshPpeGuidanceDto>> GetPpeGuidanceAsync(Guid hazardId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPpeGuidanceQuery(hazardId), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>Returns every NIOSH hazard profile whose ChemicalName/CASNumber contains <paramref name="keyword"/>, or an empty list on failure.</summary>
    public async Task<IReadOnlyList<NioshHazardProfileDto>> SearchHazardsAsync(string keyword, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchNioshHazardsQuery(keyword, Skip: 0, Take: 500), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>Returns every NIOSH hazard profile mapped to <paramref name="module"/>, or an empty list on failure.</summary>
    public async Task<IReadOnlyList<NioshHazardProfileDto>> GetHazardsByModuleAsync(string module, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNioshHazardsByModuleQuery(module), cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }
}
