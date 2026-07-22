// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           SafetyReportingCategory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of routine safety audit / incident reporting categories per the
//                  PlantSafety Program Structure Routine Safety Audits & Incident Reporting component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Enums;

/// <summary>Classification of routine safety audit / incident reporting categories.</summary>
public enum SafetyReportingCategory
{
    /// <summary>Daily/weekly floor walkthrough.</summary>
    FloorWalkthrough = 0,

    /// <summary>Near-miss report.</summary>
    NearMissReport = 1,

    /// <summary>Incident investigation.</summary>
    IncidentInvestigation = 2,
}
