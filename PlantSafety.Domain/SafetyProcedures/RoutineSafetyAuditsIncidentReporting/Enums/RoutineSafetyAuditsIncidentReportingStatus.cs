// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           RoutineSafetyAuditsIncidentReportingStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real audit-cycle lifecycle states for the RoutineSafetyAuditsIncidentReporting
//                  aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Enums;

/// <summary>Lifecycle states for the <see cref="RoutineSafetyAuditsIncidentReporting"/> aggregate root.</summary>
public enum RoutineSafetyAuditsIncidentReportingStatus
{
    /// <summary>Audit/walkthrough scheduled.</summary>
    Scheduled = 0,

    /// <summary>Conducted with no findings.</summary>
    Conducted = 1,

    /// <summary>Conducted with one or more findings logged.</summary>
    FindingsLogged = 2,

    /// <summary>Corrective action underway to remediate logged findings.</summary>
    CorrectiveActionInProgress = 3,

    /// <summary>Closed out; next cycle scheduled.</summary>
    Closed = 4,
}
