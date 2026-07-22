// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalHealthMonitoringInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a EnvironmentalHealthMonitoring lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Exceptions;

/// <summary>Thrown when a <see cref="EnvironmentalHealthMonitoring"/> lifecycle transition is not permitted.</summary>
public sealed class EnvironmentalHealthMonitoringInvalidStatusTransitionException : EnvironmentalHealthMonitoringDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public EnvironmentalHealthMonitoringInvalidStatusTransitionException(
        EnvironmentalHealthMonitoringId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'EnvironmentalHealthMonitoring' {id} from '{from}' to '{to}'.",
            "EnvironmentalHealthMonitoring.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}