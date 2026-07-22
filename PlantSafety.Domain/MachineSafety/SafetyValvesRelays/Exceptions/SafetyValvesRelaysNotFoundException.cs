// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           SafetyValvesRelaysNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a SafetyValvesRelays aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Exceptions;

/// <summary>Thrown when a <see cref="SafetyValvesRelays"/> aggregate lookup finds no match.</summary>
public sealed class SafetyValvesRelaysNotFoundException : SafetyValvesRelaysDomainException
{
    /// <summary>Creates a not-found exception for the given SafetyValvesRelays identity.</summary>
    public SafetyValvesRelaysNotFoundException(SafetyValvesRelaysId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "SafetyValvesRelays with id '{id}' was not found.",
            "SafetyValvesRelays.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}