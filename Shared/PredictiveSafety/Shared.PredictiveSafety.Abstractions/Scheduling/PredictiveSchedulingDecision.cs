// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Scheduling
//  File:           PredictiveSchedulingDecision.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The deterministic output of a
//                  scheduling prediction.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Scheduling;

/// <summary>The deterministic output of a scheduling prediction.</summary>
public sealed record PredictiveSchedulingDecision(
    bool RequiresAcceleratedScheduling,
    PredictivePriority Priority,
    IReadOnlyList<string> AppliedRuleCodes);
