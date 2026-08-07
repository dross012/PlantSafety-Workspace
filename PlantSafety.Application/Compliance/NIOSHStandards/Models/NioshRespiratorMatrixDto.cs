// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single NIOSH respirator-selection recommendation,
//   projected from Compliance.NIOSHRespiratorMatrix. Reference data, not a regulatory requirement.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.NIOSHStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.NIOSHRespiratorMatrix row.</summary>
public sealed record NioshRespiratorMatrixDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid MatrixId { get; init; }

    /// <summary>The Compliance.NIOSHHazardProfile this recommendation applies to.</summary>
    public required Guid HazardId { get; init; }

    /// <summary>Type of hazard the recommendation addresses (e.g. "Particulate", "Gas/Vapor").</summary>
    public string? HazardType { get; init; }

    /// <summary>Airborne concentration range this recommendation applies to.</summary>
    public string? ConcentrationRange { get; init; }

    /// <summary>Recommended respirator type/class.</summary>
    public required string RecommendedRespirator { get; init; }

    /// <summary>Additional notes/caveats.</summary>
    public string? Notes { get; init; }
}
