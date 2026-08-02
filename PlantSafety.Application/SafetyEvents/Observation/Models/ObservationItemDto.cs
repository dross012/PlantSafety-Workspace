// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// UPDATED: 2026-08-02 -- Phase 4 full build-out: additively extended with the fields needed to
//          back GetObservationByIdQuery (Location/Description/CorrectiveActionRequired/
//          FollowUpDueAtUtc/UpdatedAtUtc/UpdatedBy/ETag), sourced from the new
//          usp_ObservationItems_GetById proc. All new members are optional so the pre-existing
//          GetObservationQuery list read (backed by usp_ObservationItems_GetObservations, which
//          does not project these columns) continues to bind this same DTO unchanged.
// FILE DESCRIPTION: Read-model item DTO for a single safety observation.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyEvents.Observation.Models;

/// <summary>A single safety observation recorded by an observer.</summary>
public sealed record ObservationItemDto
{
    /// <summary>Observation identifier.</summary>
    public required Guid ObservationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Observation classification (e.g., SafeAct/AtRiskAct/SafeCondition/AtRiskCondition).</summary>
    public required string ObservationType { get; init; }

    /// <summary>Observation category (e.g., PPE/Housekeeping/Ergonomics/Procedure).</summary>
    public required string Category { get; init; }

    /// <summary>Current observation status (e.g., Open/ActionAssigned/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the observation was made.</summary>
    public required DateTime ObservedAtUtc { get; init; }

    /// <summary>Identity of the user who made the observation.</summary>
    public required Guid ObservedBy { get; init; }

    /// <summary>Physical location the observation was made at.</summary>
    public string? Location { get; init; }

    /// <summary>Narrative description of what was observed.</summary>
    public string? Description { get; init; }

    /// <summary>Indicates whether corrective action is required as a result of this observation.</summary>
    public bool? CorrectiveActionRequired { get; init; }

    /// <summary>UTC timestamp any required follow-up is due.</summary>
    public DateTime? FollowUpDueAtUtc { get; init; }

    /// <summary>UTC timestamp the record was last updated.</summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>Identity of the user who last updated this record.</summary>
    public Guid? UpdatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
