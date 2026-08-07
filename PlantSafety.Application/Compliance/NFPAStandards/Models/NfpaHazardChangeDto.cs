// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.NFPA_HazardChangeLog row,
//   written by tools/NfpaReferenceImport's scheduled diff pass. This is the "hazard profile
//   changed" signal other modules poll or subscribe to.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.NFPAStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.NFPA_HazardChangeLog row.</summary>
public sealed record NfpaHazardChangeDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid ChangeLogId { get; init; }

    /// <summary>The Compliance.NFPA_HazardProfile this change applies to.</summary>
    public required Guid HazardId { get; init; }

    /// <summary>NFPA code number.</summary>
    public required string CodeNumber { get; init; }

    /// <summary>Section citation.</summary>
    public required string SectionNumber { get; init; }

    /// <summary>"New", "Revised", or "Rescinded".</summary>
    public required string ChangeType { get; init; }

    /// <summary>SHA-256 hash of the section text before this change, when known.</summary>
    public string? PreviousTextHash { get; init; }

    /// <summary>SHA-256 hash of the section text after this change.</summary>
    public string? NewTextHash { get; init; }

    /// <summary>UTC instant the updater detected this change.</summary>
    public required DateTime DetectedAtUtc { get; init; }

    /// <summary>Short human-readable summary.</summary>
    public string? Summary { get; init; }
}
