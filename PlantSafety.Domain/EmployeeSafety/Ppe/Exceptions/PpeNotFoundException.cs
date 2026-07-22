// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           PpeNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Ppe aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.Exceptions;

/// <summary>Thrown when a <see cref="Ppe"/> aggregate lookup finds no match.</summary>
public sealed class PpeNotFoundException : PpeDomainException
{
    /// <summary>Creates a not-found exception for the given Ppe identity.</summary>
    public PpeNotFoundException(PpeId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Ppe with id '{id}' was not found.",
            "Ppe.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}