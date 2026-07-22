// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single EmergencyDrillPlanning record. Emergency response and drill planning: fire evacuation, chemical spill, severe weather, alarm testing, participation tracking.
// SchemaVersion: 1

namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Models;

/// <summary>A single EmergencyDrillPlanning read-model record.</summary>
public sealed record EmergencyDrillPlanningItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EmergencyDrillPlanningId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>DrillType.</summary>
    public required  string DrillType { get; init; }

    /// <summary>ShiftCode.</summary>
    public required  string ShiftCode { get; init; }

    /// <summary>ScheduledAtUtc.</summary>
    public DateTime? ScheduledAtUtc { get; init; }

    /// <summary>ParticipationCount.</summary>
    public required  int ParticipationCount { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}