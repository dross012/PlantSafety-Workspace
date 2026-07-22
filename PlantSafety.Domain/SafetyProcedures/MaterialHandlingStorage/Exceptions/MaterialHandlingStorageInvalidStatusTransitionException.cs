// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingStorageInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a MaterialHandlingStorage lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Exceptions;

/// <summary>Thrown when a <see cref="MaterialHandlingStorage"/> lifecycle transition is not permitted.</summary>
public sealed class MaterialHandlingStorageInvalidStatusTransitionException : MaterialHandlingStorageDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public MaterialHandlingStorageInvalidStatusTransitionException(
        MaterialHandlingStorageId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'MaterialHandlingStorage' {id} from '{from}' to '{to}'.",
            "MaterialHandlingStorage.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}