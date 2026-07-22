// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           SafetyValvesRelaysFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing SafetyValvesRelays aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Factories;

/// <summary>Default factory implementation for constructing <see cref="SafetyValvesRelays"/> aggregates.</summary>
public sealed class SafetyValvesRelaysFactory : ISafetyValvesRelaysFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}