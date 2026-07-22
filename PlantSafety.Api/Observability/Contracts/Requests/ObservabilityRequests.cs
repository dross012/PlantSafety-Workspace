// ============================================================================
// File: ObservabilityRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Observability V1 controller (AutomatedIncidentDetection,
//              EnvironmentalAssetMonitoring, FacilityTrafficMonitoring,
//              MachineProcessMonitoring, PPEBehavioralMonitoring).
// ============================================================================

namespace PlantSafety.Api.Observability.Contracts.Requests;

/// <summary>Request body for recording a new sensor reading for a MachineProcessMonitoring channel.</summary>
public sealed record RecordMachineSensorReadingRequest
{
    /// <summary>The recorded sensor reading value.</summary>
    public required decimal Reading { get; init; }

    /// <summary>Lower threshold bound.</summary>
    public required decimal ThresholdMin { get; init; }

    /// <summary>Upper threshold bound.</summary>
    public required decimal ThresholdMax { get; init; }
}

/// <summary>Request body for raising a process anomaly flag for a MachineProcessMonitoring channel.</summary>
public sealed record RaiseMachineAnomalyFlagRequest
{
    /// <summary>Description of the observed anomaly (unexpected startup, bypassed guard, disabled interlock).</summary>
    public required string AnomalyDescription { get; init; }
}

/// <summary>Request body for notifying the assigned supervisor of an AutomatedIncidentDetection.</summary>
public sealed record NotifySupervisorOfDetectionRequest
{
    /// <summary>Identity of the supervisor to notify.</summary>
    public required Guid SupervisorUserId { get; init; }
}

/// <summary>Request body for escalating an AutomatedIncidentDetection into a formal incident record.</summary>
public sealed record EscalateDetectionToIncidentRequest
{
    /// <summary>Identity of the created Incident Management record.</summary>
    public required Guid IncidentReferenceId { get; init; }
}

/// <summary>Request body for dismissing an AutomatedIncidentDetection as a false positive.</summary>
public sealed record DismissDetectionRequest
{
    /// <summary>Reason the detection is being dismissed.</summary>
    public required string Reason { get; init; }
}

/// <summary>Request body for a bulk Observability operation.</summary>
public sealed record BulkUpdateObservabilityRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifiers of the Observability resources to operate on. Maximum 500 per request.</summary>
    public required IReadOnlyList<Guid> ResourceIds { get; init; }

    /// <summary>The operation to apply (e.g. "Close", "Approve", "Reassign").</summary>
    public required string Operation { get; init; }

    /// <summary>Optional operation-specific key-value data.</summary>
    public Dictionary<string, string>? Parameters { get; init; }
}

/// <summary>Query-string bound filters for the Observability read-model list endpoints.</summary>
public sealed record ObservabilityListRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the date range (UTC). Defaults to 90 days before now.</summary>
    public DateTime? From { get; init; }

    /// <summary>Inclusive end of the date range (UTC). Defaults to 90 days after now.</summary>
    public DateTime? To { get; init; }
}
