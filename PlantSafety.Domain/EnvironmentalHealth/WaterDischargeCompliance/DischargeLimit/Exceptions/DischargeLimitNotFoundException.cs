// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Domain
//  File:           DischargeLimitNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a DischargeLimit aggregate lookup finds no match. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Exceptions;

/// <summary>Thrown when a <see cref="DischargeLimit"/> aggregate lookup finds no match.</summary>
public sealed class DischargeLimitNotFoundException : DischargeLimitDomainException
{
    /// <summary>Creates a not-found exception for the given DischargeLimit identity.</summary>
    public DischargeLimitNotFoundException(DischargeLimitId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"DischargeLimit with id '{id}' was not found.",
            "DischargeLimit.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}