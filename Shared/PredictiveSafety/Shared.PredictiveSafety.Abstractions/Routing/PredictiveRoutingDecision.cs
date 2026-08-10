// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Routing
//  File:           PredictiveRoutingDecision.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The deterministic output of a
//                  routing prediction.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Routing;

/// <summary>The deterministic output of a routing prediction.</summary>
public sealed record PredictiveRoutingDecision(
    string EventCode,
    Guid FacilityId,
    bool RequiresPriorityRouting,
    PredictivePriority Priority,
    IReadOnlyList<string> AppliedRuleCodes);
