// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.OSHAStandardChangeLog row,
//   written by tools/OshaStandardsImport's scheduled diff pass. This is the "regulation
//   changed" signal other modules poll or subscribe to.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.OSHAStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.OSHAStandardChangeLog row.</summary>
public sealed record OSHAStandardChangeDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid ChangeLogId { get; init; }

    /// <summary>The Compliance.OSHAStandard this change applies to.</summary>
    public required Guid OSHAStandardId { get; init; }

    /// <summary>CFR part number.</summary>
    public required int PartNumber { get; init; }

    /// <summary>Section citation.</summary>
    public required string Section { get; init; }

    /// <summary>"New", "Revised", or "Rescinded".</summary>
    public required string ChangeType { get; init; }

    /// <summary>Status before this change, when known.</summary>
    public string? PreviousStatus { get; init; }

    /// <summary>Status after this change.</summary>
    public string? NewStatus { get; init; }

    /// <summary>UTC instant the updater detected this change.</summary>
    public required DateTime DetectedAtUtc { get; init; }

    /// <summary>Short human-readable summary.</summary>
    public string? Summary { get; init; }
}
