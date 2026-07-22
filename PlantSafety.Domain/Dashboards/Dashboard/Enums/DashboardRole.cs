// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardRole.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Simple classification enum for "4. Role-Based Dashboards" viewer roles. Pure
//                  vocabulary with no behavior — satisfies the Simple Enum branch of the Enums
//                  Rule (2.7).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Dashboard.Enums;

/// <summary>Identifies the primary viewer role a <see cref="Aggregates.Dashboard"/> is designed for.</summary>
public enum DashboardRole
{
    /// <summary>Executives: Executive Safety Overview, Full Program Master Dashboard.</summary>
    Executive = 0,

    /// <summary>Safety Managers: Risk Management, Incident &amp; Near-Miss, PPE, Machine Safety, Observability, Permits, Alerts, Culture, Training.</summary>
    SafetyManager = 1,

    /// <summary>Supervisors / Line Leads: Scheduling &amp; Fatigue, Checklists &amp; Audits, Material Handling &amp; Traffic, Environmental &amp; Facility.</summary>
    Supervisor = 2,

    /// <summary>Maintenance / Engineering: Machine Safety &amp; PM, Observability, Electrical Safety, Quality &amp; Process Safety.</summary>
    MaintenanceEngineering = 3,

    /// <summary>Contractor Coordinators: Contractor Safety, PTW, Training &amp; Certification.</summary>
    ContractorCoordinator = 4,
}
