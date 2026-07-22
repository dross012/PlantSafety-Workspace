// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           LotoProceduresNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a LotoProcedures aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.Exceptions;

/// <summary>Thrown when a <see cref="LotoProcedures"/> aggregate lookup finds no match.</summary>
public sealed class LotoProceduresNotFoundException : LotoProceduresDomainException
{
    /// <summary>Creates a not-found exception for the given LotoProcedures identity.</summary>
    public LotoProceduresNotFoundException(LotoProceduresId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "LotoProcedures with id '{id}' was not found.",
            "LotoProcedures.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}