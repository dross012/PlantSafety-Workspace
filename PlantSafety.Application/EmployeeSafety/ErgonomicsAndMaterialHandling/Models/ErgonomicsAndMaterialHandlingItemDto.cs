// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single ErgonomicsAndMaterialHandling record.
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.ErgonomicsAndMaterialHandling.Models;

/// <summary>A single ErgonomicsAndMaterialHandling record (Ergonomics and material handling controls reduce musculoskeletal injuries caused by repetitive motion, awkward posture, or heavy lifting through adjustable workstations, mechanical lifting aids, and job rotation.).</summary>
public sealed record ErgonomicsAndMaterialHandlingItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid ErgonomicsAndMaterialHandlingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Assessment Title.</summary>
    public required string AssessmentTitle { get; init; }

    /// <summary>Classification: RepetitiveMotion, AwkwardPosture, HeavyLifting.</summary>
    public required string HazardCategory { get; init; }

    /// <summary>Workstation Location.</summary>
    public required string WorkstationLocation { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string ErgonomicsStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}