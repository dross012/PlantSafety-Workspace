// ============================================================================
//  Project:        Enterprise.RulesEngine
//  Module:         Abstractions.Engine
//  File:           EnterpriseRuleEvaluationResult.cs
//  Created:        2026-08-08
//  Description:    Phase D.5: the result of evaluating one EnterpriseRule
//                  against a fact set -- whether it matched and, if so,
//                  the actions it triggered. Companion output type for
//                  EnterpriseRulesEngine.Evaluate; not in Path D's
//                  original file list but required to express its return
//                  value explicitly (mirrors WorkflowDecision's precedent
//                  in Phase C.7).
// ============================================================================
using Enterprise.RulesEngine.Abstractions.Actions;
using Enterprise.RulesEngine.Abstractions.Ids;

namespace Enterprise.RulesEngine.Abstractions.Engine;

/// <summary>The result of evaluating one <c>EnterpriseRule</c> against a fact set.</summary>
public sealed record EnterpriseRuleEvaluationResult(
    EnterpriseRuleId RuleId,
    string RuleName,
    bool Matched,
    IReadOnlyList<EnterpriseRuleAction> TriggeredActions);
