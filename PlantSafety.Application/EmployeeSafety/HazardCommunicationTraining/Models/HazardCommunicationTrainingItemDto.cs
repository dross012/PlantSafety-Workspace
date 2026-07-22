// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single HazardCommunicationTraining record.
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.HazardCommunicationTraining.Models;

/// <summary>A single HazardCommunicationTraining record (Hazard communication and training ensures employees understand workplace hazards via GHS chemical labeling, accessible Safety Data Sheets (SDS), and training on chemical handling and emergency response.).</summary>
public sealed record HazardCommunicationTrainingItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid HazardCommunicationTrainingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Program Title.</summary>
    public required string ProgramTitle { get; init; }

    /// <summary>Classification: Chemical, Physical, Biological.</summary>
    public required string HazardClass { get; init; }

    /// <summary>Sds Reference.</summary>
    public required string SdsReference { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string TrainingStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}