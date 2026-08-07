// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single NIOSH PPE guidance entry, projected from
//   Compliance.NIOSHPPEGuidance. Reference data, not a regulatory requirement.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.NIOSHStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.NIOSHPPEGuidance row.</summary>
public sealed record NioshPpeGuidanceDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid GuidanceId { get; init; }

    /// <summary>The Compliance.NIOSHHazardProfile this guidance applies to.</summary>
    public required Guid HazardId { get; init; }

    /// <summary>Type of PPE (e.g. "Gloves", "Eye Protection", "Protective Clothing").</summary>
    public required string PPEType { get; init; }

    /// <summary>Performance criteria the PPE should meet.</summary>
    public string? PerformanceCriteria { get; init; }

    /// <summary>Known limitations of the recommended PPE.</summary>
    public string? Limitations { get; init; }
}
