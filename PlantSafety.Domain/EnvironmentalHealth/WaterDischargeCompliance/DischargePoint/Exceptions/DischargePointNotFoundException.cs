// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
//  Layer:          Domain
//  File:           DischargePointNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a DischargePoint aggregate lookup finds no match. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Exceptions;

/// <summary>Thrown when a <see cref="DischargePoint"/> aggregate lookup finds no match.</summary>
public sealed class DischargePointNotFoundException : DischargePointDomainException
{
    /// <summary>Creates a not-found exception for the given DischargePoint identity.</summary>
    public DischargePointNotFoundException(DischargePointId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"DischargePoint with id '{id}' was not found.",
            "DischargePoint.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}