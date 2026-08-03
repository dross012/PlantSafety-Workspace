// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           SafetyTelemetryEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Canonical, normalized per-site telemetry record produced by
//                  IGlobalSafetyTelemetryNormalizer from the platform's REAL per-site event
//                  sources (AccidentManagement.Events.AccidentCreatedEvent,
//                  SafetyEvents.IncidentManagement.Events.IncidentManagementCreatedEvent,
//                  Observability.Telemetry.ValueObjects.MachineStateSnapshot,
//                  EmployeeSafety.EnvironmentalHealthMonitoring measurement events, and
//                  Operational.ProductionTracking.Events.ProductionRunCompletedEvent). This is the
//                  single canonical shape IGlobalSafetyKpiEngine/IGlobalSafetyAnomalyDetector
//                  operate on, regardless of which real source produced it -- see
//                  GlobalSafetyTelemetryNormalizer.SourceModule constants for provenance.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;

/// <summary>Canonical, source-agnostic per-site telemetry record used as pipeline input.</summary>
public sealed record SafetyTelemetryEvent
{
    /// <summary>Tenant that owns the originating record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site the telemetry pertains to.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Name of the real source module this record was normalized from (see GlobalSafetyTelemetryNormalizer.SourceModule* constants).</summary>
    public required string SourceModule { get; init; }

    /// <summary>UTC instant the underlying real event occurred.</summary>
    public required UtcTimestamp OccurredAtUtc { get; init; }

    /// <summary>Recordable incident count contributed by this record (0 or 1 for a single incident/accident event).</summary>
    public int RecordableIncidentCount { get; init; }

    /// <summary>Lost-time incident count contributed by this record.</summary>
    public int LostTimeIncidentCount { get; init; }

    /// <summary>Hours worked contributed by this record, when known (from ProductionTracking run duration/headcount proxies).</summary>
    public decimal? HoursWorkedContribution { get; init; }

    /// <summary>Worker exposure-level score, when this record originated from EnvironmentalHealthMonitoring.</summary>
    public decimal? ExposureLevelScore { get; init; }

    /// <summary>Machine family/identifier, when this record originated from Observability.Telemetry.</summary>
    public string? MachineFamily { get; init; }

    /// <summary>Indicates the source machine-state observation was degraded (non-nominal), when applicable.</summary>
    public bool? MachineStateDegraded { get; init; }
}
