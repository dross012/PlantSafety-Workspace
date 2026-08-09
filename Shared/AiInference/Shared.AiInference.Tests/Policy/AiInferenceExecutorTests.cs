// ============================================================================
//  Project:        Shared.AiInference.Tests
//  File:           AiInferenceExecutorTests.cs
//  Created:        2026-08-09
// ============================================================================
using AwesomeAssertions;

using Shared.AiInference.Abstractions.Policy;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Deterministic.Abstractions.Random;

using Xunit;

namespace Shared.AiInference.Tests.Policy;

public sealed class AiInferenceExecutorTests
{
    private static readonly IClock Clock = new FixedClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly IRandomProvider RandomProvider = new SeededRandomProvider(42);

    [Fact]
    public async Task ExecuteAsync_InferenceSucceeds_ReturnsRealOutputAndNoFallback()
    {
        AiInferenceResult<int> result = await AiInferenceExecutor.ExecuteAsync(
            input: 5,
            inference: x => x * 2,
            fallback: _ => -1,
            policy: AiInferencePolicy.Standard,
            modelName: "DoubleModel",
            correlationId: "corr-1",
            serializeInput: x => x.ToString(),
            serializeOutput: x => x.ToString(),
            clock: Clock,
            randomProvider: RandomProvider);

        result.Output.Should().Be(10);
        result.UsedFallback.Should().BeFalse();
        result.TimedOut.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_InferenceThrows_UsesFallback()
    {
        AiInferenceResult<int> result = await AiInferenceExecutor.ExecuteAsync(
            input: 5,
            inference: _ => throw new InvalidOperationException("boom"),
            fallback: x => x,
            policy: AiInferencePolicy.Standard,
            modelName: "ThrowingModel",
            correlationId: "corr-2",
            serializeInput: x => x.ToString(),
            serializeOutput: x => x.ToString(),
            clock: Clock,
            randomProvider: RandomProvider);

        result.Output.Should().Be(5);
        result.UsedFallback.Should().BeTrue();
        result.TimedOut.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_InferenceExceedsTimeout_UsesFallbackAndReportsTimedOut()
    {
        AiInferenceResult<int> result = await AiInferenceExecutor.ExecuteAsync(
            input: 5,
            inference: x => { Thread.Sleep(200); return x; },
            fallback: _ => -1,
            policy: new AiInferencePolicy(TimeSpan.FromMilliseconds(20)),
            modelName: "SlowModel",
            correlationId: "corr-3",
            serializeInput: x => x.ToString(),
            serializeOutput: x => x.ToString(),
            clock: Clock,
            randomProvider: RandomProvider);

        result.Output.Should().Be(-1);
        result.UsedFallback.Should().BeTrue();
        result.TimedOut.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Envelope_CarriesModelNameCorrelationIdAndHashes()
    {
        AiInferenceResult<int> result = await AiInferenceExecutor.ExecuteAsync(
            input: 7,
            inference: x => x + 1,
            fallback: _ => -1,
            policy: AiInferencePolicy.Standard,
            modelName: "IncrementModel",
            correlationId: "corr-4",
            serializeInput: x => x.ToString(),
            serializeOutput: x => x.ToString(),
            clock: Clock,
            randomProvider: RandomProvider);

        result.Envelope.ModelName.Should().Be("IncrementModel");
        result.Envelope.CorrelationId.Should().Be("corr-4");
        result.Envelope.InputHash.Should().NotBeNullOrWhiteSpace();
        result.Envelope.OutputHash.Should().NotBeNullOrWhiteSpace();
        result.Envelope.GeneratedAtUtc.Should().Be(Clock.UtcNow);
    }
}
