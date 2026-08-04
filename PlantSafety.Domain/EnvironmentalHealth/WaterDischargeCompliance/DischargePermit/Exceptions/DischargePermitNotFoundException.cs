// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
//  Layer:          Domain
//  File:           DischargePermitNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a DischargePermit aggregate lookup finds no match. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Exceptions;

/// <summary>Thrown when a <see cref="DischargePermit"/> aggregate lookup finds no match.</summary>
public sealed class DischargePermitNotFoundException : DischargePermitDomainException
{
    /// <summary>Creates a not-found exception for the given DischargePermit identity.</summary>
    public DischargePermitNotFoundException(DischargePermitId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"DischargePermit with id '{id}' was not found.",
            "DischargePermit.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}