// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           ISafetyValvesRelaysFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing SafetyValvesRelays aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Interfaces;

/// <summary>Factory interface for constructing <see cref="SafetyValvesRelays"/> aggregates.</summary>
public interface ISafetyValvesRelaysFactory
{
    /// <summary>Creates a new SafetyValvesRelays aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}