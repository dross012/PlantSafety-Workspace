// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           ErgonomicsAndMaterialHandlingInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ErgonomicsAndMaterialHandling lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Exceptions;

/// <summary>Thrown when a <see cref="ErgonomicsAndMaterialHandling"/> lifecycle transition is not permitted.</summary>
public sealed class ErgonomicsAndMaterialHandlingInvalidStatusTransitionException : ErgonomicsAndMaterialHandlingDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ErgonomicsAndMaterialHandlingInvalidStatusTransitionException(
        ErgonomicsAndMaterialHandlingId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'ErgonomicsAndMaterialHandling' {id} from '{from}' to '{to}'.",
            "ErgonomicsAndMaterialHandling.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}