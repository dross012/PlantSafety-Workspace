// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// UPDATED: 2026-08-05 — Corrected to mirror the real backend
//   PlantSafety.Application.Alerts.Models.AlertDto shape exactly (the prior
//   version used fabricated field names — Description/AlertType/SourceModule/
//   SubjectEntityType/RaisedAtUtc/UpdatedAtUtc/CorrelationId — that do not
//   exist on the wire and deserialized as empty/default). Backs both
//   GET v1/alerts/{id} and the items of GET v1/alerts/history.
// FILE DESCRIPTION: Client-side DTO for a single Alert entity.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Alerts.Models;

/// <summary>
/// Full alert detail DTO returned by the Alerts API (mirrors the real
/// <c>PlantSafety.Application.Alerts.Models.AlertDto</c> read-model exactly).
/// </summary>
public sealed class AlertDto
{
    /// <summary>Gets the unique alert identifier.</summary>
    public Guid AlertId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the current lifecycle status (e.g. Triggered/Acknowledged/Dispatched/Escalated/Resolved/Closed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the catalog category code (e.g. "LifeSafetyEmergency").</summary>
    public string CategoryCode { get; init; } = string.Empty;

    /// <summary>Gets the catalog category display name.</summary>
    public string CategoryDisplayName { get; init; } = string.Empty;

    /// <summary>Gets the severity code (Critical/High/Medium/Low).</summary>
    public string SeverityCode { get; init; } = string.Empty;

    /// <summary>Gets the origin that triggered the alert (Sensor/ComputerVision/Wearable/PanicButton/Human/System).</summary>
    public string SourceType { get; init; } = string.Empty;

    /// <summary>Gets the short human-readable alert title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the zone/area name where the alert occurred.</summary>
    public string LocationZoneName { get; init; } = string.Empty;

    /// <summary>Gets the optional GPS latitude.</summary>
    public double? Latitude { get; init; }

    /// <summary>Gets the optional GPS longitude.</summary>
    public double? Longitude { get; init; }

    /// <summary>Gets the UTC instant the alert was triggered.</summary>
    public DateTime TriggeredAtUtc { get; init; }

    /// <summary>Gets the UTC instant the alert was acknowledged, if acknowledged.</summary>
    public DateTime? AcknowledgedAtUtc { get; init; }

    /// <summary>Gets the UTC instant the alert was resolved, if resolved.</summary>
    public DateTime? ResolvedAtUtc { get; init; }

    /// <summary>Gets the UTC instant the alert was closed, if closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>Gets the close-out verification notes, if closed.</summary>
    public string? CloseOutVerification { get; init; }

    /// <summary>Gets a value indicating whether this alert has been acknowledged.</summary>
    public bool IsAcknowledged => AcknowledgedAtUtc.HasValue;
}
