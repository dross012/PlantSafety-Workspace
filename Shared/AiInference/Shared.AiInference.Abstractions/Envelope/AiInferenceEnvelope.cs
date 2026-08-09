// ============================================================================
//  Project:        Shared.AiInference
//  Module:         Abstractions.Envelope
//  File:           AiInferenceEnvelope.cs
//  Created:        2026-08-09
//  Description:    Phase 2.9 (AI Engine Stabilization) deliverable: the AI
//                  inference envelope. Every AI.* engine in this program
//                  ("AI" meaning deterministic rule-based inference, per
//                  the program's standing "no real ML/LLM calls"
//                  instruction) is a pure function of its input --
//                  InputHash/OutputHash are literal deterministic hashes
//                  of the serialized input/output, letting two calls be
//                  compared for "did this input produce the same output"
//                  without storing full payloads, and giving a stable cache
//                  key for a future memoization layer.
// ============================================================================
namespace Shared.AiInference.Abstractions.Envelope;

/// <summary>The deterministic envelope wrapping one AI engine inference call.</summary>
public sealed record AiInferenceEnvelope(
    Guid InferenceId,
    string ModelName,
    string InputHash,
    string OutputHash,
    string CorrelationId,
    DateTime GeneratedAtUtc);
