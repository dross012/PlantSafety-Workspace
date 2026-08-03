// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Domain
//  File:           CrossSiteActionType.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Simple enum (Handbook §2.7.B) discriminating the four real, governed
//                  cross-site coordinated-action kinds the Multi-Site Coordination workflow can
//                  route: incident escalation, corrective-action rollout, lockout/tagout
//                  propagation, and maintenance coordination. Simple (not Smart) because each
//                  value carries no additional per-value metadata beyond its name -- eligibility/
//                  escalation/approval thresholds live on IMultiSiteSafetyPolicy, not on the enum
//                  itself. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.Coordination.Enums;

/// <summary>Discriminates the kind of coordinated action being routed across two or more sites.</summary>
public enum CrossSiteActionType
{
    /// <summary>Escalates (or creates) an Incident record at each affected site.</summary>
    IncidentEscalation = 0,

    /// <summary>Rolls out a Corrective/Preventive Action (CAPA) at each affected site.</summary>
    CorrectiveActionRollout = 1,

    /// <summary>Propagates a lockout/tagout action across affected sites.</summary>
    LockoutPropagation = 2,

    /// <summary>Coordinates a maintenance response (WorkOrder creation) across affected sites.</summary>
    MaintenanceCoordination = 3,
}
