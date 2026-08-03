// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Api
//  File:           ProductionTrackingRequests.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Request body contracts for every Operational.ProductionTracking command
//                  endpoint. Consolidated into one file per the sibling
//                  WorkOrderRequests.cs precedent of grouping small, closely-related request
//                  types. None of these are currently called by
//                  PlantSafety.UI.Modules.ProductionTracking.ApiClients.ProductionTrackingApiClient
//                  (that client is 100% read-only per the UI's Razor pages) -- they exist so the
//                  module's read-model tables can genuinely be populated (registering lines,
//                  starting/ending runs, recording timeline/downtime events, adding batches) ahead
//                  of a future manual-entry UI flow or ingestion-side integration. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Api.Operational.ProductionTracking.Contracts.Requests;

/// <summary>Request body for <c>POST v1/production-tracking/lines</c>.</summary>
public sealed record CreateProductionLineRequest
{
    /// <summary>Optional site scope.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable line code (format LINE-###).</summary>
    public required string LineCode { get; init; }

    /// <summary>Line display name.</summary>
    public required string LineName { get; init; }
}

/// <summary>Request body for <c>POST v1/production-tracking/runs</c>.</summary>
public sealed record StartProductionRunRequest
{
    /// <summary>Optional site scope.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable run code (format PR-###).</summary>
    public required string RunCode { get; init; }

    /// <summary>Identifier of the ProductionLine this run executes on.</summary>
    public required Guid LineId { get; init; }

    /// <summary>Display name of the line.</summary>
    public required string LineName { get; init; }

    /// <summary>Identifier of the telemetry-monitored machine driving this run.</summary>
    public required Guid MachineId { get; init; }

    /// <summary>Operator display name.</summary>
    public string? OperatorName { get; init; }

    /// <summary>Rated ideal cycle time (seconds).</summary>
    public required double IdealCycleTimeSeconds { get; init; }
}

/// <summary>Request body for <c>POST v1/production-tracking/runs/{id}/events</c>.</summary>
public sealed record RecordProductionRunEventRequest
{
    /// <summary>Event type (Start, Step, Pause, Error, Resume, Complete, or a custom downtime reason label).</summary>
    public required string RunEventType { get; init; }

    /// <summary>Description of the event.</summary>
    public string? Description { get; init; }

    /// <summary>Whether this event represents a downtime interval.</summary>
    public bool IsDowntime { get; init; }

    /// <summary>Machine-readable downtime reason code. Required when <see cref="IsDowntime"/> is true.</summary>
    public string? DowntimeReasonCode { get; init; }

    /// <summary>Downtime duration in minutes. Only meaningful when <see cref="IsDowntime"/> is true.</summary>
    public double DowntimeMinutes { get; init; }

    /// <summary>Good units produced since the last recorded event.</summary>
    public int GoodUnitsDelta { get; init; }

    /// <summary>Scrapped units produced since the last recorded event.</summary>
    public int ScrapUnitsDelta { get; init; }

    /// <summary>Optimistic-concurrency token from a prior read.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for <c>POST v1/production-tracking/runs/{id}/batches</c>.</summary>
public sealed record AddProductionBatchRequest
{
    /// <summary>Human-readable batch code (format B-###).</summary>
    public required string BatchCode { get; init; }

    /// <summary>Batch status (Pending, Active, Completed, Failed).</summary>
    public string Status { get; init; } = "Active";

    /// <summary>Optimistic-concurrency token from a prior read.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for <c>POST v1/production-tracking/runs/{id}/complete</c>.</summary>
public sealed record CompleteProductionRunRequest
{
    /// <summary>Whether the run completed successfully (true) or ended abnormally (false).</summary>
    public bool Succeeded { get; init; } = true;

    /// <summary>Optimistic-concurrency token from a prior read.</summary>
    public string? ETag { get; init; }
}
