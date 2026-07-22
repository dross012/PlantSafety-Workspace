// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           SafetyValvesRelaysInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a SafetyValvesRelays lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Exceptions;

/// <summary>Thrown when a <see cref="SafetyValvesRelays"/> lifecycle transition is not permitted.</summary>
public sealed class SafetyValvesRelaysInvalidStatusTransitionException : SafetyValvesRelaysDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public SafetyValvesRelaysInvalidStatusTransitionException(
        SafetyValvesRelaysId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'SafetyValvesRelays' {id} from '{from}' to '{to}'.",
            "SafetyValvesRelays.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}