// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single FMCSA (49 CFR) section, projected directly
//   from Compliance.FMCSAStandard. Global reference data -- no TenantId, matching the table's own
//   scoping (see FMCSAStandard.sql). FMCSA is a federal agency and 49 CFR is legally binding
//   regulation, the same legal character as OSHA's 29 CFR -- mirrors OSHAStandardItemDto's role.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.FMCSAStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.FMCSAStandard row.</summary>
public sealed record FmcsaStandardDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid StandardId { get; init; }

    /// <summary>CFR part number (e.g. "395"). Stored as text -- 49 CFR part numbers are not always purely numeric.</summary>
    public required string PartNumber { get; init; }

    /// <summary>Subpart letter/identifier, when the section falls under one.</summary>
    public string? SubpartLetter { get; init; }

    /// <summary>Section citation (e.g. "395.8").</summary>
    public required string SectionNumber { get; init; }

    /// <summary>Section heading text.</summary>
    public string? SectionTitle { get; init; }

    /// <summary>Full section text, verbatim from the source.</summary>
    public required string SectionText { get; init; }

    /// <summary>SHA-256 hash of SectionText, used to detect changes between import runs.</summary>
    public string? SectionTextHash { get; init; }

    /// <summary>Source URL the section was imported from.</summary>
    public string? SourceUrl { get; init; }

    /// <summary>When this row was first imported.</summary>
    public required DateTime CreatedOn { get; init; }
}
