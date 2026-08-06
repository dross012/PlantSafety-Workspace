// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// UPDATED: 2026-08-05 — The list page now renders the real AlertDto rows
//   returned by GET v1/alerts/history directly (no separate lightweight list
//   projection exists on the real backend), so this file is repurposed to
//   hold the write-surface request/result DTOs for the Alerts module's
//   lifecycle commands (Trigger/Dispatch/Escalate/Resolve/Close), mirroring
//   PlantSafety.Api.Alerts.Contracts.Requests.AlertsRequests.cs and
//   PlantSafety.Application.Alerts.Commands's *Result records exactly.
// FILE DESCRIPTION: Write-surface request/result DTOs for the Alerts module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.Alerts.Models;

/// <summary>Request body for triggering a new Alert (POST v1/alerts). Mirrors <c>TriggerAlertRequest</c>.</summary>
public sealed class TriggerAlertRequest
{
    /// <summary>Gets or sets the optional site scope.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Gets or sets the catalog category code (e.g. "LifeSafetyEmergency").</summary>
    [Required]
    [StringLength(100)]
    public string CategoryCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the origin that triggered the alert (Sensor/ComputerVision/Wearable/PanicButton/Human/System).</summary>
    [Required]
    [StringLength(50)]
    public string SourceTypeCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the short human-readable alert title.</summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the zone/area name where the alert occurred.</summary>
    [Required]
    [StringLength(200)]
    public string LocationZoneName { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional GPS latitude.</summary>
    [Range(-90.0, 90.0)]
    public double? Latitude { get; set; }

    /// <summary>Gets or sets the optional GPS longitude.</summary>
    [Range(-180.0, 180.0)]
    public double? Longitude { get; set; }

    /// <summary>Gets or sets the optional explicit severity override.</summary>
    public string? SeverityCode { get; set; }
}

/// <summary>Result of triggering a new Alert. Mirrors <c>TriggerAlertResult</c>.</summary>
public sealed class TriggerAlertResultDto
{
    /// <summary>Gets the identifier of the newly triggered alert.</summary>
    public Guid AlertId { get; init; }

    /// <summary>Gets the lifecycle status immediately after trigger.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the resolved severity code applied to the new alert.</summary>
    public string SeverityCode { get; init; } = string.Empty;
}

/// <summary>Result of acknowledging an Alert. Mirrors <c>AcknowledgeAlertResult</c>.</summary>
public sealed class AcknowledgeAlertResultDto
{
    /// <summary>Gets the identifier of the affected alert.</summary>
    public Guid AlertId { get; init; }

    /// <summary>Gets the lifecycle status after acknowledgement.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Request body for recording a dispatched response to an Alert (POST .../dispatch). Mirrors <c>DispatchAlertRequest</c>.</summary>
public sealed class DispatchAlertRequest
{
    /// <summary>Gets or sets the description of who/what was dispatched (e.g. "First Responders - Site A").</summary>
    [Required]
    [StringLength(200)]
    public string DispatchedTo { get; set; } = string.Empty;
}

/// <summary>Result of dispatching a response for an Alert. Mirrors <c>DispatchAlertResult</c>.</summary>
public sealed class DispatchAlertResultDto
{
    /// <summary>Gets the identifier of the affected alert.</summary>
    public Guid AlertId { get; init; }

    /// <summary>Gets the lifecycle status after dispatch.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Request body for escalating an Alert (POST .../escalate). Mirrors <c>EscalateAlertRequest</c>.</summary>
public sealed class EscalateAlertRequest
{
    /// <summary>Gets or sets the reason for escalation.</summary>
    [Required]
    [StringLength(500)]
    public string EscalationReason { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether this escalation is triggered automatically rather than by a human actor.</summary>
    public bool WasAutoEscalated { get; set; }
}

/// <summary>Result of escalating an Alert. Mirrors <c>EscalateAlertResult</c>.</summary>
public sealed class EscalateAlertResultDto
{
    /// <summary>Gets the identifier of the affected alert.</summary>
    public Guid AlertId { get; init; }

    /// <summary>Gets the lifecycle status after escalation.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Request body for resolving an Alert (POST .../resolve). Mirrors <c>ResolveAlertRequest</c>.</summary>
public sealed class ResolveAlertRequest
{
    /// <summary>Gets or sets the summary of response actions taken to resolve the hazard.</summary>
    [Required]
    [StringLength(1000)]
    public string ResponseActionsTaken { get; set; } = string.Empty;
}

/// <summary>Result of resolving an Alert. Mirrors <c>ResolveAlertResult</c>.</summary>
public sealed class ResolveAlertResultDto
{
    /// <summary>Gets the identifier of the affected alert.</summary>
    public Guid AlertId { get; init; }

    /// <summary>Gets the lifecycle status after resolution.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Request body for closing an Alert (POST .../close). Mirrors <c>CloseAlertRequest</c>.</summary>
public sealed class CloseAlertRequest
{
    /// <summary>Gets or sets the close-out verification notes.</summary>
    [Required]
    [StringLength(1000)]
    public string CloseOutVerification { get; set; } = string.Empty;
}

/// <summary>Result of closing an Alert. Mirrors <c>CloseAlertResult</c>.</summary>
public sealed class CloseAlertResultDto
{
    /// <summary>Gets the identifier of the affected alert.</summary>
    public Guid AlertId { get; init; }

    /// <summary>Gets the lifecycle status after closing.</summary>
    public string Status { get; init; } = string.Empty;
}
