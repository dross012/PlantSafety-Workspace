// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceEvaluationLifecycleRules.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the ComplianceEvaluation
//                  aggregate. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.RulesEngine.Enums;

namespace PlantSafety.Domain.Compliance.RulesEngine.Rules;

/// <summary>Defines permitted lifecycle transitions for the ComplianceEvaluation aggregate.</summary>
public static class ComplianceEvaluationLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a rule result may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordRuleResult(ComplianceEvaluationStatus current) => current == ComplianceEvaluationStatus.InProgress;

    /// <summary>Returns <see langword="true"/> when the evaluation may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(ComplianceEvaluationStatus current) => current == ComplianceEvaluationStatus.InProgress;
}
