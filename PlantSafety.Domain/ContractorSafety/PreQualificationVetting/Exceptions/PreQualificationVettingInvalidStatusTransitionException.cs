// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Domain
//  File:           PreQualificationVettingInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PreQualificationVetting lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Exceptions;

/// <summary>Thrown when a <see cref="PreQualificationVetting"/> lifecycle transition is not permitted.</summary>
public sealed class PreQualificationVettingInvalidStatusTransitionException : PreQualificationVettingDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PreQualificationVettingInvalidStatusTransitionException(
        PreQualificationVettingId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'PreQualificationVetting' {id} from '{from}' to '{to}'.",
            "PreQualificationVetting.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}