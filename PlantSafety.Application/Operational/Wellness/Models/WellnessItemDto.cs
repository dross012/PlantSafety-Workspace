// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-20
// FILE DESCRIPTION: Read-model item DTO for a single Wellness record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.Wellness.Models;

/// <summary>A single Wellness read-model record.</summary>
public sealed record WellnessItemDto
{
    /// <summary>Wellness record identifier.</summary>
    public required Guid WellnessCheckInId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the employee this wellness check-in belongs to.</summary>
    public required Guid EmployeeId { get; init; }

    /// <summary>Display name of the employee.</summary>
    public required string EmployeeName { get; init; }

    /// <summary>Type of wellness check-in (e.g. DailyCheckIn, MentalHealthSurvey, FatigueSelfAssessment).</summary>
    public required string CheckInType { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime CheckedInAtUtc { get; init; }

    /// <summary>Identity of the user who submitted this wellness check-in.</summary>
    public required Guid CheckedInBy { get; init; }
}