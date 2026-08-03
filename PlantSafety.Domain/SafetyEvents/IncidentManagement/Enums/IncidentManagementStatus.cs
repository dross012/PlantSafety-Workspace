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
//  Updated:        2026-08-03 -- Added Assigned/Escalated/Resolved to back the new incident
//                  lifecycle commands (AssignTechnician/Escalate/Resolve) exposed via
//                  IncidentsCanonicalV1Controller. Mirrors the new values written by
//                  usp_Incident_Assign/usp_Incident_Escalate/usp_Incident_Resolve. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Enums;

/// <summary>
/// Lifecycle states for the <see cref="Aggregates.IncidentManagement"/> aggregate root, grounded
/// strictly in the values actually written by [SafetyEvents].[usp_Incident_Create] (initial
/// N'Reported'), the lifecycle procedures (usp_Incident_Assign/Escalate/Resolve), and
/// [SafetyEvents].[usp_Incident_SoftDelete] (terminal N'Archived').
/// </summary>
public enum IncidentManagementStatus
{
    /// <summary>Incident has been reported and is open for investigation/updates. Initial state on Create.</summary>
    Reported = 0,

    /// <summary>Incident has been archived (soft-deleted) for long-term retention. Terminal state.</summary>
    Archived = 1,

    /// <summary>A technician/investigator has been assigned to the incident.</summary>
    Assigned = 2,

    /// <summary>Incident has been escalated (severity bump, supervisor notification).</summary>
    Escalated = 3,

    /// <summary>Incident has been resolved with recorded resolution notes.</summary>
    Resolved = 4,
}
