// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           RoutineSafetyAuditsIncidentReportingLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the RoutineSafetyAuditsIncidentReporting
//                  aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="RoutineSafetyAuditsIncidentReporting"/> aggregate.</summary>
public static class RoutineSafetyAuditsIncidentReportingLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the audit may be conducted from <paramref name="current"/>.</summary>
    public static bool CanConduct(RoutineSafetyAuditsIncidentReportingStatus current) => current == RoutineSafetyAuditsIncidentReportingStatus.Scheduled;

    /// <summary>Returns <see langword="true"/> when corrective action may begin from <paramref name="current"/>.</summary>
    public static bool CanBeginCorrectiveAction(RoutineSafetyAuditsIncidentReportingStatus current) => current == RoutineSafetyAuditsIncidentReportingStatus.FindingsLogged;

    /// <summary>Returns <see langword="true"/> when the audit cycle may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(RoutineSafetyAuditsIncidentReportingStatus current) =>
        current is RoutineSafetyAuditsIncidentReportingStatus.Conducted or RoutineSafetyAuditsIncidentReportingStatus.CorrectiveActionInProgress;
}
