// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementStatus.cs
//  Created:        2026-06-17
//  Updated:        2026-07-27 -- Replaced the generic Created/Active/Completed/Closed/Archived
//                  placeholder states (which had no basis anywhere in the codebase -- no column,
//                  procedure, or handler ever produced "Active" or "Completed") with the two
//                  states actually persisted by [SafetyEvents].[Incident]: the table's
//                  DF_Incident_Status default is N'Reported' (see usp_Incident_Create.sql) and the
//                  only other value ever written is N'Archived' (see usp_Incident_SoftDelete.sql).
//                  This removes domain-rule drift between the aggregate and its real persistence
//                  layer. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Enums;

/// <summary>
/// Lifecycle states for the <see cref="Aggregates.IncidentManagement"/> aggregate root, grounded
/// strictly in the values actually written by [SafetyEvents].[usp_Incident_Create] (initial
/// N'Reported') and [SafetyEvents].[usp_Incident_SoftDelete] (terminal N'Archived').
/// </summary>
public enum IncidentManagementStatus
{
    /// <summary>Incident has been reported and is open for investigation/updates. Initial state on Create.</summary>
    Reported = 0,

    /// <summary>Incident has been archived (soft-deleted) for long-term retention. Terminal state.</summary>
    Archived = 1,
}
