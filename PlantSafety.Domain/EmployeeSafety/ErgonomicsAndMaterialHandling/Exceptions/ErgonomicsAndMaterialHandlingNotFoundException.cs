// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           ErgonomicsAndMaterialHandlingNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ErgonomicsAndMaterialHandling aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Exceptions;

/// <summary>Thrown when a <see cref="ErgonomicsAndMaterialHandling"/> aggregate lookup finds no match.</summary>
public sealed class ErgonomicsAndMaterialHandlingNotFoundException : ErgonomicsAndMaterialHandlingDomainException
{
    /// <summary>Creates a not-found exception for the given ErgonomicsAndMaterialHandling identity.</summary>
    public ErgonomicsAndMaterialHandlingNotFoundException(ErgonomicsAndMaterialHandlingId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "ErgonomicsAndMaterialHandling with id '{id}' was not found.",
            "ErgonomicsAndMaterialHandling.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}