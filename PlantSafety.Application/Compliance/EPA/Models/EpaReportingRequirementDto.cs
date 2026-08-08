// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.EpaReportingRequirement row --
//   an EPA-mandated reporting trigger (release reporting, notifications, periodic filings),
//   optionally tied to a single EpaRegulatoryRequirement. Global reference data, not
//   tenant-scoped.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.EPA.Models;

/// <summary>Flat read-model DTO for a single Compliance.EpaReportingRequirement row.</summary>
public sealed record EpaReportingRequirementDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid ReportingRequirementId { get; init; }

    /// <summary>The EPA program this reporting requirement belongs to (e.g. "CERCLA").</summary>
    public required string ProgramCode { get; init; }

    /// <summary>Optional link back to the requirement this reporting obligation operationalizes.</summary>
    public Guid? RequirementId { get; init; }

    /// <summary>Narrative description of the event/condition that triggers the reporting obligation.</summary>
    public required string TriggerDescription { get; init; }

    /// <summary>Free-text deadline description (e.g. "within 24 hours", "annually by March 1").</summary>
    public required string DeadlineDescription { get; init; }

    /// <summary>Where the report must be filed (e.g. "National Response Center").</summary>
    public required string ReportingDestination { get; init; }

    /// <summary>Name of the regulatory form to use, when a specific form is mandated.</summary>
    public string? FormName { get; init; }

    /// <summary>When this row was created.</summary>
    public required DateTime CreatedOn { get; init; }
}
