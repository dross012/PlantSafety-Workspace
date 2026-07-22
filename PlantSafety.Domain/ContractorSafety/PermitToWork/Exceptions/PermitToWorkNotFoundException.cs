// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWorkNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PermitToWork aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Exceptions;

/// <summary>Thrown when a <see cref="PermitToWork"/> aggregate lookup finds no match.</summary>
public sealed class PermitToWorkNotFoundException : PermitToWorkDomainException
{
    /// <summary>Creates a not-found exception for the given PermitToWork identity.</summary>
    public PermitToWorkNotFoundException(PermitToWorkId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "PermitToWork with id '{id}' was not found.",
            "PermitToWork.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}