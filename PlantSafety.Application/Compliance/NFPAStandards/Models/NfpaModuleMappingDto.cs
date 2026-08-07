// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single hazard-to-module mapping row, projected
//   from Compliance.NFPA_HazardModuleMapping. Ties an NFPA hazard profile to a PlantSafety
//   module (e.g. "MachineSafety", "PermitToWork") with a severity rating and rationale.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.NFPAStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.NFPA_HazardModuleMapping row.</summary>
public sealed record NfpaModuleMappingDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid MappingId { get; init; }

    /// <summary>The Compliance.NFPA_HazardProfile this mapping applies to.</summary>
    public required Guid HazardId { get; init; }

    /// <summary>PlantSafety module this hazard is mapped to (e.g. "MachineSafety").</summary>
    public required string ModuleName { get; init; }

    /// <summary>Deterministic rule code that produced this mapping.</summary>
    public required string MappingRuleCode { get; init; }

    /// <summary>Rationale for why this hazard was mapped to this module.</summary>
    public required string Rationale { get; init; }

    /// <summary>Advisory/Low/Medium/High/Critical -- the relative severity of this mapping.</summary>
    public required string Severity { get; init; }
}
