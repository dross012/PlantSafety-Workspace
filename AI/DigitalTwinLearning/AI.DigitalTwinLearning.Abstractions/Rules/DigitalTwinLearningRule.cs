// ============================================================================
//  Project:        AI.DigitalTwinLearning
//  Module:         Abstractions.Rules
//  File:           DigitalTwinLearningRule.cs
//  Created:        2026-08-08
//  Description:    Phase E.7: a single deterministic rule contributing a
//                  model-update recommendation when a LearnedPattern
//                  satisfies its typed Condition.
// ============================================================================
using AI.DigitalTwinLearning.Abstractions.Learning;

namespace AI.DigitalTwinLearning.Abstractions.Rules;

/// <summary>A single deterministic rule contributing a model-update recommendation when a learned pattern matches.</summary>
public sealed record DigitalTwinLearningRule(
    string RuleId,
    string Name,
    Func<LearnedPattern, bool> Condition,
    string ModelUpdateRecommendation);
