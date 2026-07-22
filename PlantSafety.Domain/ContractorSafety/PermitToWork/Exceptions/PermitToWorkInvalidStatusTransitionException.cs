// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWorkInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PermitToWork lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Exceptions;

/// <summary>Thrown when a <see cref="PermitToWork"/> lifecycle transition is not permitted.</summary>
public sealed class PermitToWorkInvalidStatusTransitionException : PermitToWorkDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PermitToWorkInvalidStatusTransitionException(
        PermitToWorkId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'PermitToWork' {id} from '{from}' to '{to}'.",
            "PermitToWork.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}