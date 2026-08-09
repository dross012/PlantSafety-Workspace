// ============================================================================
//  Project:        AI.EnterpriseOrchestration
//  Module:         Abstractions.Rules
//  File:           EnterpriseOrchestrationRule.cs
//  Created:        2026-08-08
//  Description:    Phase E.9: a single deterministic rule contributing an
//                  enterprise-wide alert when its typed Condition,
//                  evaluated over every participating facility's context,
//                  is satisfied.
// ============================================================================
using AI.EnterpriseOrchestration.Abstractions.Orchestration;

namespace AI.EnterpriseOrchestration.Abstractions.Rules;

/// <summary>A single deterministic rule contributing an enterprise-wide alert when its condition is satisfied.</summary>
public sealed record EnterpriseOrchestrationRule(
    string RuleId,
    string Name,
    Func<IReadOnlyList<FacilityOrchestrationContext>, bool> Condition,
    string Alert);
