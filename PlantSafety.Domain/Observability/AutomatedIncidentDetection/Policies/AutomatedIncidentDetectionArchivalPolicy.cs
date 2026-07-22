// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for AutomatedIncidentDetection aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Policies;

/// <summary>Determines whether a <see cref="AutomatedIncidentDetection"/> aggregate is eligible for archival.</summary>
public static class AutomatedIncidentDetectionArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the AutomatedIncidentDetection may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection aggregate)
        => aggregate.Status == AutomatedIncidentDetectionStatus.Closed;
}