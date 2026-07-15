// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Client-side DTO for a single Alert entity. Maps the
//   API response from /api/v1/alerts/{id}. All string fields are non-nullable
//   with empty defaults. No domain logic present — display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Alerts.Models;

/// <summary>
/// Full alert detail DTO returned by the Alerts API for single-record queries.
/// </summary>
public sealed class AlertDto
{
    /// <summary>Gets the unique alert identifier (UUIDv7).</summary>
    public Guid AlertId { get; init; }

    /// <summary>Gets the alert title for display in list and detail views.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the alert description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the alert severity code (e.g., "Critical", "High", "Medium", "Low").</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets the alert status code (e.g., "Open", "Acknowledged", "Escalated", "Closed").</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the alert type code (e.g., "SafetyEvent", "SystemAlert", "Threshold").</summary>
    public string AlertType { get; init; } = string.Empty;

    /// <summary>Gets the module that originated this alert (e.g., "IncidentManagement").</summary>
    public string SourceModule { get; init; } = string.Empty;

    /// <summary>Gets the entity type of the alert's subject (e.g., "Worker", "Machine", "Site").</summary>
    public string SubjectEntityType { get; init; } = string.Empty;

    /// <summary>Gets the ID of the alert's subject entity.</summary>
    public Guid? SubjectEntityId { get; init; }

    /// <summary>Gets the site identifier where this alert was generated.</summary>
    public Guid SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this alert was raised.</summary>
    public DateTimeOffset RaisedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp when this alert was last updated.</summary>
    public DateTimeOffset UpdatedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp when this alert was acknowledged, or null if unacknowledged.</summary>
    public DateTimeOffset? AcknowledgedAtUtc { get; init; }

    /// <summary>Gets the display name of the user who acknowledged this alert, or null.</summary>
    public string? AcknowledgedByName { get; init; }

    /// <summary>Gets the correlation identifier for lineage display in the detail view.</summary>
    public Guid CorrelationId { get; init; }

    /// <summary>Gets a value indicating whether this alert has been acknowledged.</summary>
    public bool IsAcknowledged => AcknowledgedAtUtc.HasValue;
}
