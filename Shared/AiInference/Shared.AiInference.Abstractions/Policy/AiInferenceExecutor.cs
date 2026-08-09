// ============================================================================
//  Project:        Shared.AiInference
//  Module:         Abstractions.Policy
//  File:           AiInferenceExecutor.cs
//  Created:        2026-08-09
//  Description:    Phase 2.9 deliverable: the AI policy engine. Runs a
//                  deterministic engine's Func<TInput,TOutput> under
//                  AiInferencePolicy's timeout, falling back to a caller-
//                  supplied deterministic default if the call throws or
//                  exceeds the timeout, and always returns an
//                  AiInferenceEnvelope built from deterministic hashes of
//                  the (caller-serialized) input and whichever output was
//                  actually produced.
// ============================================================================
using Shared.AiInference.Abstractions.Envelope;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Deterministic.Abstractions.Random;

namespace Shared.AiInference.Abstractions.Policy;

/// <summary>Runs a deterministic AI engine call under timeout/fallback policy, producing its inference envelope.</summary>
public static class AiInferenceExecutor
{
    /// <summary>Executes the given inference function under the given policy.</summary>
    public static async Task<AiInferenceResult<TOutput>> ExecuteAsync<TInput, TOutput>(
        TInput input,
        Func<TInput, TOutput> inference,
        Func<TInput, TOutput> fallback,
        AiInferencePolicy policy,
        string modelName,
        string correlationId,
        Func<TInput, string> serializeInput,
        Func<TOutput, string> serializeOutput,
        IClock clock,
        IRandomProvider randomProvider)
    {
        ArgumentNullException.ThrowIfNull(inference);
        ArgumentNullException.ThrowIfNull(fallback);

        TOutput output;
        var usedFallback = false;
        var timedOut = false;

        Task<TOutput> inferenceTask = Task.Run(() => inference(input));
        try
        {
            Task completed = await Task.WhenAny(inferenceTask, Task.Delay(policy.Timeout));
            if (completed != inferenceTask)
            {
                timedOut = true;
                output = fallback(input);
                usedFallback = true;
            }
            else
            {
                output = await inferenceTask;
            }
        }
        catch (Exception)
        {
            output = fallback(input);
            usedFallback = true;
        }

        string inputHash = AiInferenceHasher.Hash(serializeInput(input));
        string outputHash = AiInferenceHasher.Hash(serializeOutput(output));
        var envelope = new AiInferenceEnvelope(randomProvider.NewGuid(), modelName, inputHash, outputHash, correlationId, clock.UtcNow);

        return new AiInferenceResult<TOutput>(output, envelope, usedFallback, timedOut);
    }
}
