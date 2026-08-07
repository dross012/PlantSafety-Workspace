// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.NIOSHHazardChangeLog row,
//   written by the NIOSH reference-data sync tool's scheduled diff pass. This is the "hazard
//   profile changed" signal other modules poll or subscribe to.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.NIOSHStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.NIOSHHazardChangeLog row.</summary>
public sealed record NioshHazardChangeDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid ChangeLogId { get; init; }

    /// <summary>The Compliance.NIOSHHazardProfile this change applies to.</summary>
    public required Guid HazardId { get; init; }

    /// <summary>Chemical name.</summary>
    public required string ChemicalName { get; init; }

    /// <summary>"New", "Revised", or "Rescinded".</summary>
    public required string ChangeType { get; init; }

    /// <summary>UTC instant the updater detected this change.</summary>
    public required DateTime DetectedAtUtc { get; init; }

    /// <summary>Short human-readable summary.</summary>
    public string? Summary { get; init; }
}
