// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalHealthMonitoringNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a EnvironmentalHealthMonitoring aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Exceptions;

/// <summary>Thrown when a <see cref="EnvironmentalHealthMonitoring"/> aggregate lookup finds no match.</summary>
public sealed class EnvironmentalHealthMonitoringNotFoundException : EnvironmentalHealthMonitoringDomainException
{
    /// <summary>Creates a not-found exception for the given EnvironmentalHealthMonitoring identity.</summary>
    public EnvironmentalHealthMonitoringNotFoundException(EnvironmentalHealthMonitoringId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "EnvironmentalHealthMonitoring with id '{id}' was not found.",
            "EnvironmentalHealthMonitoring.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}