// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single EmergencyDrillPlanning record. Emergency response and drill planning: fire evacuation, chemical spill, severe weather, alarm testing, participation tracking.
// SchemaVersion: 1
// UPDATED: 2026-07-30 -- Phase 4 full build-out: added ETag (optimistic-concurrency token) to back
//   GetEmergencyDrillPlanningByIdQuery / the lifecycle-transition commands' read-before-write flow,
//   mirroring PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto.ETag.

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

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}