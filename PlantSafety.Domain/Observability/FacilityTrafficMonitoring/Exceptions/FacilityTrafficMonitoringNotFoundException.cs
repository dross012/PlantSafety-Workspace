// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           FacilityTrafficMonitoringNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a FacilityTrafficMonitoring aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Exceptions;

/// <summary>Thrown when a <see cref="FacilityTrafficMonitoring"/> aggregate lookup finds no match.</summary>
public sealed class FacilityTrafficMonitoringNotFoundException : FacilityTrafficMonitoringDomainException
{
    /// <summary>Creates a not-found exception for the given FacilityTrafficMonitoring identity.</summary>
    public FacilityTrafficMonitoringNotFoundException(FacilityTrafficMonitoringId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"FacilityTrafficMonitoring with id '{id}' was not found.",
            "FacilityTrafficMonitoring.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}