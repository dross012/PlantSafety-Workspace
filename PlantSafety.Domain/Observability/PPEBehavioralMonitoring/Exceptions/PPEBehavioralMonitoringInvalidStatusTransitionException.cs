// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           PPEBehavioralMonitoringInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PPEBehavioralMonitoring lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Exceptions;

/// <summary>Thrown when a <see cref="PPEBehavioralMonitoring"/> lifecycle transition is not permitted.</summary>
public sealed class PPEBehavioralMonitoringInvalidStatusTransitionException : PPEBehavioralMonitoringDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PPEBehavioralMonitoringInvalidStatusTransitionException(
        PPEBehavioralMonitoringId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'PPEBehavioralMonitoring' {id} from '{from}' to '{to}'.",
            "PPEBehavioralMonitoring.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}