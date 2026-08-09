// ============================================================================
//  Project:        Shared.ErrorHandling
//  Module:         Abstractions.Policy
//  File:           IErrorPolicy.cs
//  Created:        2026-08-09
//  Description:    Phase 2.3 deliverable: the error policy engine's seam.
//                  One Evaluate call combines retry, backoff, circuit-
//                  breaker, and escalation rules into a single decision.
// ============================================================================
using Shared.ErrorHandling.Abstractions.Envelope;

namespace Shared.ErrorHandling.Abstractions.Policy;

/// <summary>A policy engine's decision for how to handle one occurrence of an error.</summary>
public sealed record ErrorPolicyDecision(bool ShouldRetry, TimeSpan RetryDelay, bool ShouldEscalate, string Reason);

/// <summary>Combines retry, backoff, circuit-breaker, and escalation rules into a single decision.</summary>
public interface IErrorPolicy
{
    /// <summary>Evaluates what should happen for the given error, on the given (1-based) attempt number.</summary>
    ErrorPolicyDecision Evaluate(ErrorEnvelope error, int attemptNumber);
}
