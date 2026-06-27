// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Domain
//  File:           ProceduresNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Procedures.Exceptions;

/// <summary>Thrown when a <see cref="Procedures"/> aggregate lookup finds no match.</summary>
public sealed class ProceduresNotFoundException : ProceduresDomainException
{
    /// <summary>Creates a not-found exception for the given Procedures identity.</summary>
    public ProceduresNotFoundException(ProceduresId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Procedures with id '{id}' was not found.",
            "Procedures.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
