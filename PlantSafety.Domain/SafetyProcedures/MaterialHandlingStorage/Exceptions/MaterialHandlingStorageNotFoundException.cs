// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingStorageNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a MaterialHandlingStorage aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Exceptions;

/// <summary>Thrown when a <see cref="MaterialHandlingStorage"/> aggregate lookup finds no match.</summary>
public sealed class MaterialHandlingStorageNotFoundException : MaterialHandlingStorageDomainException
{
    /// <summary>Creates a not-found exception for the given MaterialHandlingStorage identity.</summary>
    public MaterialHandlingStorageNotFoundException(MaterialHandlingStorageId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "MaterialHandlingStorage with id '{id}' was not found.",
            "MaterialHandlingStorage.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}