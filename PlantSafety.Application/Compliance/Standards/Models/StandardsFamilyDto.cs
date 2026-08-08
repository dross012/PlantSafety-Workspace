// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.StandardsFamily row -- one of
//   the ANSI/ASME/ISO consensus-standards families. Global reference data, not tenant-scoped.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Standards.Models;

/// <summary>Flat read-model DTO for a single Compliance.StandardsFamily row.</summary>
public sealed record StandardsFamilyDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid FamilyId { get; init; }

    /// <summary>Short family code (e.g. "ANSI", "ASME", "ISO").</summary>
    public required string FamilyCode { get; init; }

    /// <summary>Full family name.</summary>
    public required string FamilyName { get; init; }

    /// <summary>Narrative description of the family's scope.</summary>
    public string? Description { get; init; }

    /// <summary>When this row was created.</summary>
    public required DateTime CreatedOn { get; init; }
}
