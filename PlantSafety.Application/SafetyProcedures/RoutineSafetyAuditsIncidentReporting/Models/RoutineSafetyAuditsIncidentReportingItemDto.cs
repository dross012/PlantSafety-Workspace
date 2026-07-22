// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single RoutineSafetyAuditsIncidentReporting record.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Models;

/// <summary>A single RoutineSafetyAuditsIncidentReporting record (Routine safety audit and incident-reporting procedures maintain continuous improvement via floor walkthroughs, near-miss reporting, and incident investigation protocols.).</summary>
public sealed record RoutineSafetyAuditsIncidentReportingItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid RoutineSafetyAuditsIncidentReportingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Audit Title.</summary>
    public required string AuditTitle { get; init; }

    /// <summary>Classification: FloorWalkthrough, NearMissReport, IncidentInvestigation.</summary>
    public required string ReportingCategory { get; init; }

    /// <summary>Facility Area.</summary>
    public required string FacilityArea { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string AuditStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}