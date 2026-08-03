// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Read-side DTOs for the Incident SLA Monitor (UI Gaps Scoping,
//   IncidentManagement Part 2). Mirror PlantSafety.UI.Modules.IncidentManagement.Models'
//   SlaIncidentDto/SlaRiskDto/SlaTrendDto.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyEvents.SlaMonitoring.Models;

/// <summary>Single active-incident row for the SLA Monitor grid.</summary>
public sealed record SlaIncidentDto
{
    /// <summary>Incident identifier.</summary>
    public required Guid IncidentId { get; init; }

    /// <summary>Incident title.</summary>
    public required string Title { get; init; }

    /// <summary>Severity classification.</summary>
    public required string Severity { get; init; }

    /// <summary>Remaining SLA time. Negative when breached.</summary>
    public required TimeSpan SlaRemaining { get; init; }

    /// <summary>Assigned technician identifier as text, or null when unassigned.</summary>
    public string? TechnicianName { get; init; }

    /// <summary>Current workflow status.</summary>
    public required string Status { get; init; }

    /// <summary>Current escalation state (None, Escalated, SupervisorReview).</summary>
    public required string EscalationState { get; init; }
}

/// <summary>Single high-risk row for the SLA Risk Panel.</summary>
public sealed record SlaRiskDto
{
    /// <summary>Incident identifier.</summary>
    public required Guid IncidentId { get; init; }

    /// <summary>Incident title.</summary>
    public required string Title { get; init; }

    /// <summary>Severity classification.</summary>
    public required string Severity { get; init; }

    /// <summary>Remaining SLA time. Negative when breached.</summary>
    public required TimeSpan SlaRemaining { get; init; }

    /// <summary>Assigned technician identifier as text, or null when unassigned.</summary>
    public string? TechnicianName { get; init; }

    /// <summary>Deterministic recommended action (AssignTechnician, Escalate, SupervisorReview, ImmediateActionRequired, None).</summary>
    public required string RecommendedAction { get; init; }
}

/// <summary>SLA trend summary for the SLA Trend Chart.</summary>
public sealed record SlaTrendDto
{
    /// <summary>Overall SLA compliance percentage (0-100) across the trend window.</summary>
    public required double CompliancePercentage { get; init; }

    /// <summary>Count of SLA breaches within the trend window.</summary>
    public required int BreachCount { get; init; }

    /// <summary>Severity distribution (severity name to count) within the trend window.</summary>
    public required IReadOnlyDictionary<string, int> SeverityDistribution { get; init; }
}
