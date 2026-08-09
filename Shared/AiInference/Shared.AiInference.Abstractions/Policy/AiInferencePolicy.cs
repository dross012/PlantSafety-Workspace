// ============================================================================
//  Project:        Shared.AiInference
//  Module:         Abstractions.Policy
//  File:           AiInferencePolicy.cs
//  Created:        2026-08-09
//  Description:    Phase 2.9 deliverable: deterministic AI policies.
//                  Every AI.* engine in this program is a synchronous,
//                  pure, deterministic function -- there is no I/O, no
//                  randomness, no external dependency inside one, so
//                  "retry rules" have no real value here (retrying a pure
//                  function with the same input always reproduces the
//                  same result or the same exception; there is nothing
//                  transient to wait out). Timeout and fallback DO have
//                  real value -- a pathological input could still cause
//                  runaway computation, and a caller may prefer a safe
//                  default over a 500 when an engine throws on
//                  unanticipated input -- so those are what this policy
//                  actually governs.
// ============================================================================
namespace Shared.AiInference.Abstractions.Policy;

/// <summary>Deterministic timeout and fallback policy for one AI engine call.</summary>
public sealed record AiInferencePolicy(TimeSpan Timeout)
{
    /// <summary>The standard policy: a 5 second timeout.</summary>
    public static AiInferencePolicy Standard { get; } = new(TimeSpan.FromSeconds(5));
}
