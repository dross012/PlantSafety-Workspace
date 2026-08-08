// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.EpaProgram row -- one of the
//   five EPA statutory programs (RCRA, TSCA, CERCLA/SARA, CAA, CWA). Global reference data,
//   not tenant-scoped. Uses the "Epa" prefix (not bare "EPA") to avoid collision with the
//   pre-existing, unrelated Compliance.Certification.EPA / Compliance.RegulatoryCalendar.
//   Deadlines.EPA features.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.EPA.Models;

/// <summary>Flat read-model DTO for a single Compliance.EpaProgram row.</summary>
public sealed record EpaProgramDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid ProgramId { get; init; }

    /// <summary>Short program code (e.g. "RCRA", "TSCA", "CERCLA", "CAA", "CWA").</summary>
    public required string ProgramCode { get; init; }

    /// <summary>Full program name.</summary>
    public required string ProgramName { get; init; }

    /// <summary>Narrative description of the program's scope.</summary>
    public string? Description { get; init; }

    /// <summary>Primary governing statute citation (e.g. "42 U.S.C. Ch. 82").</summary>
    public string? PrimaryStatuteCitation { get; init; }

    /// <summary>When this row was created.</summary>
    public required DateTime CreatedOn { get; init; }
}
