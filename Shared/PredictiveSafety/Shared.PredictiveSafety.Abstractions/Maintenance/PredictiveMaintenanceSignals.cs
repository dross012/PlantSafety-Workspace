// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Maintenance
//  File:           PredictiveMaintenanceSignals.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The deterministic cross-module
//                  inputs PredictiveMaintenanceInferenceEngine reacts to.
//                  IsMaintenanceOverdue is drawn directly from the real
//                  Shared.MachineDomain.Abstractions.Maintenance.
//                  MachineMaintenanceProfile.IsOverdue(nowUtc) (Phase C.2) --
//                  not reimplemented here. SchedulingWorstCaseDueWithinHours
//                  is drawn from Shared.Scheduling's real SchedulingRuleDecision
//                  (Phase 14). RecentReliabilityFailureCount is the same
//                  ReliabilityHooks-derived substitute for "Diagnostics
//                  signals" PredictiveHazardSignals documents.
// ============================================================================
namespace Shared.PredictiveSafety.Abstractions.Maintenance;

/// <summary>The deterministic cross-module inputs a maintenance prediction reacts to.</summary>
public sealed record PredictiveMaintenanceSignals(
    Guid MachineId,
    bool IsMaintenanceOverdue,
    int RecentFailureCount,
    int SchedulingWorstCaseDueWithinHours,
    int RecentReliabilityFailureCount);
