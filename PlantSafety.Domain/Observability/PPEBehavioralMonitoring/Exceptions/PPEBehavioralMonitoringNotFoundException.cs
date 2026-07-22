// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           PPEBehavioralMonitoringNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PPEBehavioralMonitoring aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Exceptions;

/// <summary>Thrown when a <see cref="PPEBehavioralMonitoring"/> aggregate lookup finds no match.</summary>
public sealed class PPEBehavioralMonitoringNotFoundException : PPEBehavioralMonitoringDomainException
{
    /// <summary>Creates a not-found exception for the given PPEBehavioralMonitoring identity.</summary>
    public PPEBehavioralMonitoringNotFoundException(PPEBehavioralMonitoringId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"PPEBehavioralMonitoring with id '{id}' was not found.",
            "PPEBehavioralMonitoring.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}