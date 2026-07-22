// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           LotoProgramsInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a LotoPrograms lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.Exceptions;

/// <summary>Thrown when a <see cref="LotoPrograms"/> lifecycle transition is not permitted.</summary>
public sealed class LotoProgramsInvalidStatusTransitionException : LotoProgramsDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public LotoProgramsInvalidStatusTransitionException(
        LotoProgramsId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'LotoPrograms' {id} from '{from}' to '{to}'.",
            "LotoPrograms.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}