// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Incident Management module.
//   Mirrors the server-side IncidentDto/IncidentSummaryDto shape (see
//   PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement)
//   for the list/detail pages, plus SLA Monitor (ISM1) and Timeline (IT1)
//   wireframe DTOs. Display-only — no domain logic.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.IncidentManagement.Models;

/// <summary>Lightweight list-view DTO for an incident (mirrors server IncidentSummaryDto).</summary>
public sealed class IncidentSummaryDto
{
    /// <summary>Gets the unique incident identifier (UUIDv7).</summary>
    public Guid IncidentId { get; init; }

    /// <summary>Gets the short descriptive title of the incident.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the incident occurred.</summary>
    public DateTimeOffset IncidentOccurredAtUtc { get; init; }

    /// <summary>Gets the severity classification (Minor, Moderate, Serious, Critical).</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets the current workflow status (Reported, UnderInvestigation, Closed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the physical or logical location where the incident occurred.</summary>
    public string Location { get; init; } = string.Empty;
}

/// <summary>Full incident detail DTO (mirrors server IncidentDto, plus ID1 wireframe fields).</summary>
public sealed class IncidentDetailDto
{
    /// <summary>Gets the unique incident identifier.</summary>
    public Guid IncidentId { get; init; }

    /// <summary>Gets the short descriptive title of the incident.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the detailed description of what occurred.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the incident category.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the incident subcategory, if any.</summary>
    public string? Subcategory { get; init; }

    /// <summary>Gets the severity classification.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets the current workflow status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the physical or logical location where the incident occurred.</summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>Gets the equipment identifier associated with the incident, if any.</summary>
    public Guid? EquipmentId { get; init; }

    /// <summary>Gets the detection source (e.g. Operator, Sensor, Inspection).</summary>
    public string DetectionSource { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the incident occurred.</summary>
    public DateTimeOffset IncidentOccurredAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp when the incident was reported.</summary>
    public DateTimeOffset ReportedAtUtc { get; init; }

    /// <summary>Gets the remaining SLA time. Negative when breached.</summary>
    public TimeSpan SlaRemaining { get; init; }

    /// <summary>Gets the assigned technician display name, or null when unassigned.</summary>
    public string? AssignedTechnicianName { get; init; }

    /// <summary>Gets the assigned technician's role/title, if assigned.</summary>
    public string? AssignedTechnicianRole { get; init; }

    /// <summary>Gets the recorded root cause text, or null when not yet determined.</summary>
    public string? RootCause { get; init; }

    /// <summary>Gets the resolution notes, or null when unresolved.</summary>
    public string? ResolutionNotes { get; init; }

    /// <summary>Gets the UTC timestamp when the incident was closed, if applicable.</summary>
    public DateTimeOffset? ClosedAtUtc { get; init; }

    /// <summary>
    /// Gets the optimistic-concurrency row version. Required by the Update/Delete lifecycle
    /// commands. FIXED 2026-08-05 (UI write-surface pass): this field was previously missing
    /// from the client-side DTO even though the server's IncidentDto always returns it —
    /// without it, Edit/Delete could never be wired up because the required ETag body field
    /// had nothing to bind to.
    /// </summary>
    public string? ETag { get; init; }

    /// <summary>Gets the site where the incident occurred.</summary>
    public Guid SiteId { get; init; }

    /// <summary>Gets the reason recorded for the most recent escalation, if any.</summary>
    public string? EscalationReason { get; init; }

    /// <summary>Gets the number of times this incident has been escalated.</summary>
    public int EscalationCount { get; init; }
}

/// <summary>Request body for creating a new Incident (mirrors server CreateIncidentRequest).</summary>
public sealed class CreateIncidentRequest
{
    /// <summary>Site where the incident occurred.</summary>
    public Guid SiteId { get; set; }

    /// <summary>Short descriptive title. Max 200 characters.</summary>
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Detailed narrative description. Max 4000 characters.</summary>
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(4000, ErrorMessage = "Description must not exceed 4000 characters.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the incident actually occurred. Cannot be in the future.</summary>
    [System.ComponentModel.DataAnnotations.Required]
    public DateTimeOffset IncidentOccurredAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Initial severity classification (Minor, Moderate, Serious, Critical).</summary>
    [System.ComponentModel.DataAnnotations.Required]
    public string Severity { get; set; } = "Minor";

    /// <summary>Physical or logical location description. Max 500 characters.</summary>
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(500, ErrorMessage = "Location must not exceed 500 characters.")]
    public string Location { get; set; } = string.Empty;

    /// <summary>Worker who is reporting the incident.</summary>
    [System.ComponentModel.DataAnnotations.Required]
    public Guid ReportedByWorkerId { get; set; }

    /// <summary>Worker assigned as investigator, if known at time of creation.</summary>
    public Guid? AssignedInvestigatorId { get; set; }
}

/// <summary>Request body for updating an existing Incident (mirrors server UpdateIncidentRequest). Null fields mean no change.</summary>
public sealed class UpdateIncidentRequest
{
    /// <summary>Updated title. Max 200 characters. Null means no change.</summary>
    [System.ComponentModel.DataAnnotations.StringLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
    public string? Title { get; set; }

    /// <summary>Updated description. Max 4000 characters. Null means no change.</summary>
    [System.ComponentModel.DataAnnotations.StringLength(4000, ErrorMessage = "Description must not exceed 4000 characters.")]
    public string? Description { get; set; }

    /// <summary>Updated severity. Null means no change.</summary>
    public string? Severity { get; set; }

    /// <summary>Updated location. Null means no change.</summary>
    [System.ComponentModel.DataAnnotations.StringLength(500, ErrorMessage = "Location must not exceed 500 characters.")]
    public string? Location { get; set; }

    /// <summary>Updated investigator assignment. Null means no change; empty Guid means unassign.</summary>
    public Guid? AssignedInvestigatorId { get; set; }

    /// <summary>Optimistic concurrency token from the last read of this Incident. Required by the server.</summary>
    public string? ETag { get; set; }
}

/// <summary>Request body for soft-deleting an Incident (mirrors server DeleteIncidentRequest).</summary>
public sealed class DeleteIncidentRequest
{
    /// <summary>Reason for deletion, required for audit trail. Max 1000 characters.</summary>
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(1000, ErrorMessage = "Deletion reason must not exceed 1000 characters.")]
    public string DeletionReason { get; set; } = string.Empty;

    /// <summary>Optimistic concurrency token from the last read of this Incident. Required by the server.</summary>
    public string? ETag { get; set; }
}

/// <summary>Single incident timeline entry (Volume ID1/IT1).</summary>
public sealed class IncidentTimelineEntryDto
{
    /// <summary>Gets the entry identifier.</summary>
    public Guid EntryId { get; init; }

    /// <summary>Gets the entry timestamp (UTC).</summary>
    public DateTimeOffset TimestampUtc { get; init; }

    /// <summary>Gets the actor who performed/generated this entry.</summary>
    public string Actor { get; init; } = string.Empty;

    /// <summary>Gets the event type (StatusChange, OperatorAction, SensorAlert, AutomatedRule, Note, Escalation, Resolution).</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>Gets the severity associated with this event.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets a description of the event.</summary>
    public string Description { get; init; } = string.Empty;
}

/// <summary>Single active-incident row for the SLA Monitor grid (Volume ISM1).</summary>
public sealed class SlaIncidentDto
{
    /// <summary>Gets the incident identifier.</summary>
    public Guid IncidentId { get; init; }

    /// <summary>Gets the incident title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the severity classification.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets the remaining SLA time. Negative when breached.</summary>
    public TimeSpan SlaRemaining { get; init; }

    /// <summary>Gets the assigned technician display name, or null when unassigned.</summary>
    public string? TechnicianName { get; init; }

    /// <summary>Gets the current workflow status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the current escalation state (None, Escalated, SupervisorReview).</summary>
    public string EscalationState { get; init; } = string.Empty;
}

/// <summary>Single high-risk row for the SLA Risk Panel (Volume ISM1).</summary>
public sealed class SlaRiskDto
{
    /// <summary>Gets the incident identifier.</summary>
    public Guid IncidentId { get; init; }

    /// <summary>Gets the incident title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the severity classification.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets the remaining SLA time. Negative when breached.</summary>
    public TimeSpan SlaRemaining { get; init; }

    /// <summary>Gets the assigned technician display name, or null when unassigned.</summary>
    public string? TechnicianName { get; init; }

    /// <summary>Gets the deterministic recommended action (AssignTechnician, Escalate, SupervisorReview, ImmediateActionRequired, None).</summary>
    public string RecommendedAction { get; init; } = string.Empty;
}

/// <summary>SLA trend summary for the SLA Trend Chart (Volume ISM1 Tier 3).</summary>
public sealed class SlaTrendDto
{
    /// <summary>Gets the overall SLA compliance percentage (0-100).</summary>
    public double CompliancePercentage { get; init; }

    /// <summary>Gets the count of SLA breaches within the trend window.</summary>
    public int BreachCount { get; init; }

    /// <summary>Gets the severity distribution (severity name to count).</summary>
    public IReadOnlyDictionary<string, int> SeverityDistribution { get; init; } = new Dictionary<string, int>();
}
