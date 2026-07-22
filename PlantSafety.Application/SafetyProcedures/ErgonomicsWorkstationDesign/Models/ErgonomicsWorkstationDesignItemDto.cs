// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single ErgonomicsWorkstationDesign record.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.ErgonomicsWorkstationDesign.Models;

/// <summary>A single ErgonomicsWorkstationDesign record (Ergonomics and workstation-design procedures minimize repetitive-motion injuries via adjustable workstation height/reach/layout, mechanical lifting aids, and job rotation.).</summary>
public sealed record ErgonomicsWorkstationDesignItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid ErgonomicsWorkstationDesignId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Procedure Title.</summary>
    public required string ProcedureTitle { get; init; }

    /// <summary>Classification: WorkstationHeight, ReachAndLayout, JobRotation.</summary>
    public required string DesignCategory { get; init; }

    /// <summary>Workstation Location.</summary>
    public required string WorkstationLocation { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string ProcedureStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}