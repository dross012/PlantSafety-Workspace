// ============================================================================
// File: AlertsRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Alerts V1 controller.
// ============================================================================

namespace PlantSafety.Api.Alerts.Contracts.Requests;

/// <summary>Request body for triggering a new Alert.</summary>
public sealed record TriggerAlertRequest
{
    /// <summary>Optional site scope.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Catalog category code (e.g. "LifeSafetyEmergency").</summary>
    public required string CategoryCode { get; init; }

    /// <summary>Origin that triggered the alert (Sensor/ComputerVision/Wearable/PanicButton/Human/System).</summary>
    public required string SourceTypeCode { get; init; }

    /// <summary>Short human-readable alert title.</summary>
    public required string Title { get; init; }

    /// <summary>Zone/area name where the alert occurred.</summary>
    public required string LocationZoneName { get; init; }

    /// <summary>Optional GPS latitude.</summary>
    public double? Latitude { get; init; }

    /// <summary>Optional GPS longitude.</summary>
    public double? Longitude { get; init; }

    /// <summary>Optional explicit severity override.</summary>
    public string? SeverityCode { get; init; }
}

/// <summary>Request body for acknowledging an Alert. No domain fields beyond the route-bound AlertId; actor is resolved from the caller's identity.</summary>
public sealed record AcknowledgeAlertRequest;

/// <summary>Request body for recording a dispatched response to an Alert.</summary>
public sealed record DispatchAlertRequest
{
    /// <summary>Description of who/what was dispatched (e.g. "First Responders - Site A").</summary>
    public required string DispatchedTo { get; init; }
}

/// <summary>Request body for escalating an Alert.</summary>
public sealed record EscalateAlertRequest
{
    /// <summary>Reason for escalation.</summary>
    public required string EscalationReason { get; init; }

    /// <summary>True when this escalation is triggered automatically rather than by a human actor.</summary>
    public bool WasAutoEscalated { get; init; }
}

/// <summary>Request body for resolving an Alert.</summary>
public sealed record ResolveAlertRequest
{
    /// <summary>Summary of response actions taken to resolve the hazard.</summary>
    public required string ResponseActionsTaken { get; init; }
}

/// <summary>Request body for closing an Alert.</summary>
public sealed record CloseAlertRequest
{
    /// <summary>Close-out verification notes.</summary>
    public required string CloseOutVerification { get; init; }
}

/// <summary>Request body for recording a multi-channel delivery attempt for an Alert.</summary>
public sealed record RecordAlertDeliveryRequest
{
    /// <summary>Delivery channel used (e.g. "Sms", "PaSpeaker").</summary>
    public required string ChannelCode { get; init; }

    /// <summary>Whether the delivery attempt succeeded.</summary>
    public required bool Succeeded { get; init; }

    /// <summary>Sanitized failure reason, when the attempt did not succeed.</summary>
    public string? FailureReason { get; init; }
}

/// <summary>Request body for recording a response action taken against an Alert.</summary>
public sealed record RecordAlertResponseActionRequest
{
    /// <summary>Short description of the action taken.</summary>
    public required string ActionDescription { get; init; }

    /// <summary>Optional free-text notes.</summary>
    public string? Notes { get; init; }
}

/// <summary>Query-string bound filters/paging for listing Alert history.</summary>
public sealed record ListAlertHistoryRequest
{
    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the date range (UTC).</summary>
    public required DateTime From { get; init; }

    /// <summary>Inclusive end of the date range (UTC).</summary>
    public required DateTime To { get; init; }

    /// <summary>1-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page.</summary>
    public int PageSize { get; init; } = 25;
}
