// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.Telemetry
//  Layer:          Domain
//  File:           MachineStateSnapshot.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Immutable point-in-time capture of a monitored machine's operating state,
//                  sourced from Observability.MachineProcessMonitoring telemetry ingestion.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Observability.Telemetry.Enums;

namespace PlantSafety.Domain.Observability.Telemetry.ValueObjects;

/// <summary>An immutable point-in-time capture of a monitored machine's operating state.</summary>
public sealed record MachineStateSnapshot
{
    /// <summary>Identity of the monitored machine.</summary>
    public required Guid MachineId { get; init; }

    /// <summary>Operating state observed at <see cref="ObservedAtUtc"/>.</summary>
    public required MachineOperatingState State { get; init; }

    /// <summary>UTC instant the state was observed.</summary>
    public required UtcTimestamp ObservedAtUtc { get; init; }
}
