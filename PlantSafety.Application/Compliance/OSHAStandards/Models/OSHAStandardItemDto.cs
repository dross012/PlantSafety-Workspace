// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single CFR Title 29 (OSHA) section, projected
//   directly from Compliance.OSHAStandard. Global reference data -- no TenantId, matching the
//   table's own scoping (see OSHAStandard.sql).
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.OSHAStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.OSHAStandard row.</summary>
public sealed record OSHAStandardItemDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid OSHAStandardId { get; init; }

    /// <summary>CFR title number. Always 29 for data imported by tools/OshaStandardsImport.</summary>
    public required int TitleNumber { get; init; }

    /// <summary>CFR part number (e.g. 1910).</summary>
    public required int PartNumber { get; init; }

    /// <summary>Part heading text, when present in the source XML.</summary>
    public string? PartTitle { get; init; }

    /// <summary>Subpart letter/identifier (e.g. "Z"), when the section falls under one.</summary>
    public string? Subpart { get; init; }

    /// <summary>Subpart heading text, when present.</summary>
    public string? SubpartTitle { get; init; }

    /// <summary>Section citation (e.g. "&#167; 1910.1200").</summary>
    public required string Section { get; init; }

    /// <summary>Section heading text.</summary>
    public string? SectionTitle { get; init; }

    /// <summary>Full section text, verbatim from the eCFR source.</summary>
    public required string SectionText { get; init; }

    /// <summary>Most recent amendment date parsed from the section's citation, when present.</summary>
    public DateOnly? EffectiveDate { get; init; }

    /// <summary>"Active", "Reserved", or "Rescinded" -- see OSHAStandard.sql/tools/OshaStandardsImport.</summary>
    public required string Status { get; init; }

    /// <summary>Source URL the section was imported from.</summary>
    public string? SourceUrl { get; init; }

    /// <summary>When this row was first imported.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>When this row was last updated by the sync tool, if ever.</summary>
    public DateTime? UpdatedAtUtc { get; init; }
}
