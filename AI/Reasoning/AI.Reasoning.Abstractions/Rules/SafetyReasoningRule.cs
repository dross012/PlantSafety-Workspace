// ============================================================================
//  Project:        AI.Reasoning
//  Module:         Abstractions.Rules
//  File:           SafetyReasoningRule.cs
//  Created:        2026-08-08
//  Description:    Phase E.1: a single deterministic reasoning rule --
//                  Condition is a plain, compile-time-typed predicate over
//                  SafetyReasoningContext (not a reflection-based or
//                  string-driven expression), so every rule is explicit,
//                  fully typed C#. When Condition returns true, Conclusion
//                  is added to the reasoning session's output.
// ============================================================================
using AI.Reasoning.Abstractions.Context;

namespace AI.Reasoning.Abstractions.Rules;

/// <summary>A single deterministic reasoning rule: a typed predicate plus the conclusion it contributes when satisfied.</summary>
public sealed record SafetyReasoningRule(
    string RuleId,
    string Name,
    Func<SafetyReasoningContext, bool> Condition,
    string Conclusion);
