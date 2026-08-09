// ============================================================================
//  Project:        AI.Reasoning
//  Module:         Abstractions.Context
//  File:           SafetyReasoningContext.cs
//  Created:        2026-08-08
//  Description:    Phase E.1: the envelope a reasoning session runs
//                  within -- a SafetyReasoningInput plus the session
//                  identity/timestamp metadata rules and the engine need,
//                  but that isn't itself part of the domain signal set.
//                  Kept as a distinct type from SafetyReasoningInput so
//                  input data (what) and session metadata (when/which
//                  run) don't get conflated.
// ============================================================================
namespace AI.Reasoning.Abstractions.Context;

/// <summary>The envelope a safety reasoning session runs within.</summary>
public sealed record SafetyReasoningContext(
    Guid ReasoningSessionId,
    SafetyReasoningInput Input,
    DateTime EvaluatedAtUtc);
