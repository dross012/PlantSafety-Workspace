// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// UPDATED: 2026-08-02 -- Phase 4 full build-out: additively extended with the fields needed to
//          back GetEmergencyManagementByIdQuery (Location/CasualtyCount/ResponseTeamNotified/
//          StabilizedAtUtc/ResolvedAtUtc/UpdatedAtUtc/UpdatedBy/ETag), sourced from the new
//          usp_EmergencyManagementItems_GetById proc. All new members are optional so the
//          pre-existing GetEmergencyManagementQuery list read (backed by
//          usp_EmergencyManagementItems_GetEmergencies, which does not project these columns)
//          continues to bind this same DTO unchanged.
// FILE DESCRIPTION: Read-model item DTO for a single emergency management event.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Models;

/// <summary>A single emergency event tracked by the EmergencyManagement module.</summary>
public sealed record EmergencyManagementItemDto
{
    /// <summary>Emergency event identifier.</summary>
    public required Guid EmergencyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type of emergency (e.g., Fire/ChemicalRelease/MedicalEmergency/SevereWeather).</summary>
    public required string EmergencyType { get; init; }

    /// <summary>Severity classification (e.g., Low/Medium/High/Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Current emergency status (e.g., Reported/Responding/Contained/Resolved).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the emergency was reported.</summary>
    public required DateTime ReportedAtUtc { get; init; }

    /// <summary>Identity of the user who reported the emergency.</summary>
    public required Guid ReportedBy { get; init; }

    /// <summary>Physical location the emergency was declared at.</summary>
    public string? Location { get; init; }

    /// <summary>Count of casualties/injuries associated with this emergency.</summary>
    public int? CasualtyCount { get; init; }

    /// <summary>Indicates whether the emergency response team has been notified.</summary>
    public bool? ResponseTeamNotified { get; init; }

    /// <summary>UTC timestamp the emergency was stabilized.</summary>
    public DateTime? StabilizedAtUtc { get; init; }

    /// <summary>UTC timestamp the emergency was resolved.</summary>
    public DateTime? ResolvedAtUtc { get; init; }

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
