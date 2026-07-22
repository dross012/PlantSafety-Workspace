// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PMScheduling aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Exceptions;

/// <summary>Thrown when a <see cref="PMScheduling"/> aggregate lookup finds no match.</summary>
public sealed class PMSchedulingNotFoundException : PMSchedulingDomainException
{
    /// <summary>Creates a not-found exception for the given PMScheduling identity.</summary>
    public PMSchedulingNotFoundException(PMSchedulingId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"PMScheduling with id '{id}' was not found.",
            "PMScheduling.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}