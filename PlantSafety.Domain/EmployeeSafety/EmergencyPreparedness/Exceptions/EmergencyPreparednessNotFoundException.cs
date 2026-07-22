// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           EmergencyPreparednessNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a EmergencyPreparedness aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Exceptions;

/// <summary>Thrown when a <see cref="EmergencyPreparedness"/> aggregate lookup finds no match.</summary>
public sealed class EmergencyPreparednessNotFoundException : EmergencyPreparednessDomainException
{
    /// <summary>Creates a not-found exception for the given EmergencyPreparedness identity.</summary>
    public EmergencyPreparednessNotFoundException(EmergencyPreparednessId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "EmergencyPreparedness with id '{id}' was not found.",
            "EmergencyPreparedness.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}