// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single recommended-control row, projected from
//   Compliance.NFPA_ControlMatrix. Recommended controls, not mandatory requirements -- NFPA
//   codes are voluntary consensus guidance.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.NFPAStandards.Models;

/// <summary>Flat read-model DTO for a single Compliance.NFPA_ControlMatrix row.</summary>
public sealed record NfpaControlMatrixDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid MatrixId { get; init; }

    /// <summary>The Compliance.NFPA_HazardProfile this control applies to.</summary>
    public required Guid HazardId { get; init; }

    /// <summary>Category of control (e.g. "Engineering", "Administrative", "PPE").</summary>
    public required string ControlType { get; init; }

    /// <summary>Description of the recommended control.</summary>
    public required string ControlDescription { get; init; }

    /// <summary>Advisory/Low/Medium/High/Critical -- the relative importance of this control, not a compliance severity.</summary>
    public required string Severity { get; init; }

    /// <summary>Rationale for why this control is recommended.</summary>
    public required string Rationale { get; init; }
}
