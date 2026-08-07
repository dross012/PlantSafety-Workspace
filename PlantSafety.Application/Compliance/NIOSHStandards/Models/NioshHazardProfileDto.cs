// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single NIOSH chemical hazard profile, projected
//   directly from Compliance.NIOSHHazardProfile. Reference data, not a regulatory requirement --
//   global, no TenantId, matching the table's own scoping.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.NIOSHStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.NIOSHHazardProfile row.</summary>
public sealed record NioshHazardProfileDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid HazardId { get; init; }

    /// <summary>Chemical name.</summary>
    public required string ChemicalName { get; init; }

    /// <summary>CAS Registry Number.</summary>
    public required string CASNumber { get; init; }

    /// <summary>NIOSH Recommended Exposure Limit, time-weighted average, when published.</summary>
    public double? RelTwa { get; init; }

    /// <summary>NIOSH Recommended Exposure Limit, short-term exposure limit, when published.</summary>
    public double? RelStel { get; init; }

    /// <summary>Known/documented health effects of exposure.</summary>
    public string? HealthEffects { get; init; }

    /// <summary>Recommended engineering/administrative controls.</summary>
    public string? RecommendedControls { get; init; }

    /// <summary>Recommended personal protective equipment.</summary>
    public string? PpeRecommendations { get; init; }

    /// <summary>Recommended atmospheric monitoring approach.</summary>
    public string? AtmosphericMonitoring { get; init; }

    /// <summary>Ergonomic factors associated with handling this hazard, when documented.</summary>
    public string? ErgonomicFactors { get; init; }

    /// <summary>Source URL the profile was imported from.</summary>
    public string? SourceUrl { get; init; }
}
