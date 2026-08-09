// ============================================================================
//  Project:        AI.Reasoning
//  Module:         Abstractions.Engine
//  File:           SafetyReasoningEngine.cs
//  Created:        2026-08-08
//  Description:    Phase E.1 (Path E: AI-Driven Safety Automation): the
//                  entry point for deterministic safety reasoning. Pure
//                  function, no side effects, no reflection -- evaluates
//                  every supplied SafetyReasoningRule against a
//                  SafetyReasoningContext, in order, and aggregates a
//                  worst-case severity across the input's SeverityLevel
//                  and DigitalTwinSimulatedSeverity signals.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Severity;

using AI.Reasoning.Abstractions.Context;
using AI.Reasoning.Abstractions.Rules;

namespace AI.Reasoning.Abstractions.Engine;

/// <summary>Deterministically reasons over a safety context using a supplied rule set.</summary>
public static class SafetyReasoningEngine
{
    /// <summary>Evaluates every rule against the given context and returns the aggregated reasoning output.</summary>
    public static SafetyReasoningOutput Reason(SafetyReasoningContext context, IReadOnlyList<SafetyReasoningRule> rules)
    {
        IReadOnlyList<string> conclusions = rules
            .Where(rule => rule.Condition(context))
            .Select(rule => rule.Conclusion)
            .ToArray();

        SeverityLevel overallSeverity = context.Input.SeverityLevel >= context.Input.DigitalTwinSimulatedSeverity
            ? context.Input.SeverityLevel
            : context.Input.DigitalTwinSimulatedSeverity;

        return new SafetyReasoningOutput(context.ReasoningSessionId, conclusions, overallSeverity, context.EvaluatedAtUtc);
    }
}
