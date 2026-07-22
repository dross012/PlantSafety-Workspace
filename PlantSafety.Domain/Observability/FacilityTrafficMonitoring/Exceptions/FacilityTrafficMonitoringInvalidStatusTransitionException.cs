// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           FacilityTrafficMonitoringInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a FacilityTrafficMonitoring lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Exceptions;

/// <summary>Thrown when a <see cref="FacilityTrafficMonitoring"/> lifecycle transition is not permitted.</summary>
public sealed class FacilityTrafficMonitoringInvalidStatusTransitionException : FacilityTrafficMonitoringDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public FacilityTrafficMonitoringInvalidStatusTransitionException(
        FacilityTrafficMonitoringId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'FacilityTrafficMonitoring' {id} from '{from}' to '{to}'.",
            "FacilityTrafficMonitoring.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}