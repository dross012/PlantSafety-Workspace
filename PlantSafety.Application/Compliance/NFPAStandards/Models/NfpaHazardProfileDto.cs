// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single NFPA hazard profile, projected directly
//   from Compliance.NFPA_HazardProfile. NFPA is not a government agency and publishes voluntary
//   consensus codes/standards, not legally binding regulations (unlike OSHA's CFR Title 29) --
//   this DTO therefore models a "hazard profile with recommended controls", never a "regulatory
//   requirement". Global reference data -- no TenantId, matching the table's own scoping (see
//   NFPA_HazardProfile.sql).
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.NFPAStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.NFPA_HazardProfile row.</summary>
public sealed record NfpaHazardProfileDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid HazardId { get; init; }

    /// <summary>NFPA code number (e.g. "70" for the National Electrical Code, "51B" for Hot Work).</summary>
    public required string CodeNumber { get; init; }

    /// <summary>Section citation within the code (e.g. "110.26").</summary>
    public required string SectionNumber { get; init; }

    /// <summary>Short hazard name/title.</summary>
    public required string HazardName { get; init; }

    /// <summary>Narrative description of the hazard.</summary>
    public required string HazardDescription { get; init; }

    /// <summary>Recommended controls for this hazard, when documented. Advisory, not mandatory.</summary>
    public string? RecommendedControls { get; init; }

    /// <summary>Recommended PPE for this hazard, when documented.</summary>
    public string? PPERecommendations { get; init; }

    /// <summary>Electrical safety guidance specific to this hazard (e.g. NFPA 70/70E context), when documented.</summary>
    public string? ElectricalSafetyGuidance { get; init; }

    /// <summary>Hot work guidance specific to this hazard (e.g. NFPA 51B context), when documented.</summary>
    public string? HotWorkGuidance { get; init; }

    /// <summary>Flammable/combustible materials guidance specific to this hazard, when documented.</summary>
    public string? FlammableMaterialsGuidance { get; init; }

    /// <summary>Source URL the hazard profile was imported from.</summary>
    public string? SourceUrl { get; init; }

    /// <summary>When this row was first imported.</summary>
    public required DateTime CreatedOn { get; init; }

    /// <summary>SHA-256 hash of the source section text, used to detect revisions. Null if never hashed.</summary>
    public string? SectionTextHash { get; init; }
}
