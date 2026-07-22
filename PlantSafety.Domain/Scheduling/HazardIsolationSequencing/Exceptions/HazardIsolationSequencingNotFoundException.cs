// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           HazardIsolationSequencingNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HazardIsolationSequencing aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Exceptions;

/// <summary>Thrown when a <see cref="HazardIsolationSequencing"/> aggregate lookup finds no match.</summary>
public sealed class HazardIsolationSequencingNotFoundException : HazardIsolationSequencingDomainException
{
    /// <summary>Creates a not-found exception for the given HazardIsolationSequencing identity.</summary>
    public HazardIsolationSequencingNotFoundException(HazardIsolationSequencingId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"HazardIsolationSequencing with id '{id}' was not found.",
            "HazardIsolationSequencing.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}