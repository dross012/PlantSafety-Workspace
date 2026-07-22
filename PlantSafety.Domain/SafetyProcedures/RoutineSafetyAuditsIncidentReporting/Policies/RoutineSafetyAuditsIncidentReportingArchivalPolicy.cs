// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           RoutineSafetyAuditsIncidentReportingArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for RoutineSafetyAuditsIncidentReporting
//                  aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Policies;

/// <summary>Determines whether a <see cref="RoutineSafetyAuditsIncidentReporting"/> aggregate is eligible for archival.</summary>
public static class RoutineSafetyAuditsIncidentReportingArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the audit record may be archived (closed only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting aggregate)
        => aggregate.Status == RoutineSafetyAuditsIncidentReportingStatus.Closed;
}
