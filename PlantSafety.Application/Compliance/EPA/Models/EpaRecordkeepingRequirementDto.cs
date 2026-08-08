// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.EpaRecordkeepingRequirement
//   row -- an EPA-mandated recordkeeping/retention requirement, optionally tied to a single
//   EpaRegulatoryRequirement. Global reference data, not tenant-scoped.
//   RequiredDataElements is projected from the underlying NVARCHAR(MAX) JSON array column
//   into a strongly-typed list by the Infrastructure-layer repository.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.EPA.Models;

/// <summary>Flat read-model DTO for a single Compliance.EpaRecordkeepingRequirement row.</summary>
public sealed record EpaRecordkeepingRequirementDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid RecordkeepingRequirementId { get; init; }

    /// <summary>The EPA program this recordkeeping requirement belongs to (e.g. "RCRA").</summary>
    public required string ProgramCode { get; init; }

    /// <summary>Optional link back to the requirement this recordkeeping obligation operationalizes.</summary>
    public Guid? RequirementId { get; init; }

    /// <summary>Minimum number of years records must be retained.</summary>
    public required int RetentionPeriodYears { get; init; }

    /// <summary>The set of data elements that must be captured and retained.</summary>
    public required IReadOnlyList<string> RequiredDataElements { get; init; }

    /// <summary>Narrative description of the recordkeeping obligation.</summary>
    public string? Description { get; init; }

    /// <summary>When this row was created.</summary>
    public required DateTime CreatedOn { get; init; }
}
