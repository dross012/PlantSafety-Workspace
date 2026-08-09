// ============================================================================
//  Project:        Shared.AiInference
//  Module:         Abstractions.Policy
//  File:           AiInferenceResult.cs
//  Created:        2026-08-09
//  Description:    The result of one governed AI inference call: the
//                  output (real or fallback), the envelope, and whether
//                  the fallback rule fired.
// ============================================================================
using Shared.AiInference.Abstractions.Envelope;

namespace Shared.AiInference.Abstractions.Policy;

/// <summary>The result of one governed AI inference call.</summary>
public sealed record AiInferenceResult<TOutput>(TOutput Output, AiInferenceEnvelope Envelope, bool UsedFallback, bool TimedOut);
