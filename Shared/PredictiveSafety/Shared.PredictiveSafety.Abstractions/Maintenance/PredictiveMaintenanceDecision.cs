// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Maintenance
//  File:           PredictiveMaintenanceDecision.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The deterministic output of a
//                  maintenance prediction.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Maintenance;

/// <summary>The deterministic output of a maintenance prediction.</summary>
public sealed record PredictiveMaintenanceDecision(
    Guid MachineId,
    bool RequiresImmediateMaintenance,
    PredictivePriority Priority,
    IReadOnlyList<string> AppliedRuleCodes);
