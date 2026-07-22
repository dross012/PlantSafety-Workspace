// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           HazardIsolationSequencingInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HazardIsolationSequencing lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Exceptions;

/// <summary>Thrown when a <see cref="HazardIsolationSequencing"/> lifecycle transition is not permitted.</summary>
public sealed class HazardIsolationSequencingInvalidStatusTransitionException : HazardIsolationSequencingDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public HazardIsolationSequencingInvalidStatusTransitionException(
        HazardIsolationSequencingId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'HazardIsolationSequencing' {id} from '{from}' to '{to}'.",
            "HazardIsolationSequencing.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}