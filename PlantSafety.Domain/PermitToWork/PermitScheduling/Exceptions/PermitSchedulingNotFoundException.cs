// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Domain
//  File:           PermitSchedulingNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PermitScheduling aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitScheduling.Exceptions;

/// <summary>Thrown when a <see cref="PermitScheduling"/> aggregate lookup finds no match.</summary>
public sealed class PermitSchedulingNotFoundException : PermitSchedulingDomainException
{
    /// <summary>Creates a not-found exception for the given PermitScheduling identity.</summary>
    public PermitSchedulingNotFoundException(PermitSchedulingId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"PermitScheduling with id '{id}' was not found.",
            "PermitScheduling.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}