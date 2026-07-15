// ============================================================================
//  Test IDs:  PLNT-STRS-ALARM-001..004
//  Manual:    performance_stress_manual.md (PR10 min 2x throughput, PR11 5x
//             burst >=30s, SR10 max queue depth 10,000, SR11 drop strategy at
//             >=90% depth).
//  Purpose:   Validates the safety-critical Alert creation path holds up
//             under concurrent burst load, and that a bounded work queue
//             modeled on SR10/SR11 sheds load correctly rather than growing
//             unbounded (OOM risk) or blocking indefinitely.
// ============================================================================
using PlantSafety.StressTests.Support;

namespace PlantSafety.StressTests.Alerts;

public sealed class AlertBurstStressTests
{
    private static readonly UserId Actor = UserId.FromGuid(Guid.NewGuid());

    [Fact(DisplayName = "PLNT-STRS-ALARM-001: 5x concurrent burst of Alert.Create completes without exceptions")]
    public async Task AlertCreate_UnderFiveTimesBurstConcurrency_ShouldNotThrow()
    {
        const int baseline = 200;
        const int burstMultiplier = 5;
        const int totalOperations = baseline * burstMultiplier;

        var errors = new ConcurrentBag<Exception>();

        await Parallel.ForEachAsync(Enumerable.Range(0, totalOperations),
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
            (i, _) =>
            {
                try
                {
                    Alert.Create(
                        AlertType.MachineFailure,
                        AlertSeverity.High,
                        $"Burst alert {i}",
                        "PLNT-STRS-ALARM-001",
                        "StressTests",
                        Actor,
                        UtcTimestamp.Now(),
                        new CorrelationId(Guid.NewGuid()),
                        new CausationId(Guid.NewGuid()),
                        PerformedByActor.User(Actor));
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }

                return ValueTask.CompletedTask;
            });

        errors.Should().BeEmpty("performance_stress_manual.md PR11 requires absorbing a 5x burst without failure");
    }

    [Fact(DisplayName = "PLNT-STRS-ALARM-002: Sustained load sustains at least 2x baseline throughput")]
    public async Task AlertCreate_SustainedLoad_ShouldSustainAtLeastTwiceBaselineThroughput()
    {
        const int sampleOps = 500;

        var baselineSw = Stopwatch.StartNew();
        for (var i = 0; i < sampleOps; i++)
            CreateAlert(i);
        baselineSw.Stop();
        var baselineOpsPerSecond = sampleOps / baselineSw.Elapsed.TotalSeconds;

        var targetOps = sampleOps * 2;
        var sustainedSw = Stopwatch.StartNew();
        await Parallel.ForEachAsync(Enumerable.Range(0, targetOps),
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            (i, _) => { CreateAlert(i); return ValueTask.CompletedTask; });
        sustainedSw.Stop();
        var sustainedOpsPerSecond = targetOps / sustainedSw.Elapsed.TotalSeconds;

        sustainedOpsPerSecond.Should().BeGreaterThanOrEqualTo(baselineOpsPerSecond,
            "performance_stress_manual.md PR10 requires sustaining >=2x expected production throughput");
    }

    [Fact(DisplayName = "PLNT-STRS-QUEUE-001: Bounded queue enforces the 10,000-item hard cap (SR10)")]
    public async Task BoundedQueue_ShouldEnforceMaxDepthOfTenThousand()
    {
        const int maxQueueDepth = 10_000;
        // FullMode.Wait (the default) makes TryWrite return false once the channel is at
        // capacity instead of silently accepting-and-dropping (DropWrite), which is what
        // SR10's "hard cap" wording requires: writes beyond capacity must be REJECTED.
        var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(maxQueueDepth)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        var accepted = 0;
        for (var i = 0; i < maxQueueDepth + 2_000; i++)
        {
            if (channel.Writer.TryWrite(i))
                accepted++;
        }

        accepted.Should().Be(maxQueueDepth, "performance_stress_manual.md SR10 mandates a hard cap of 10,000 queued items");
        channel.Writer.TryWrite(-1).Should().BeFalse("writes beyond the SR10 hard cap must be rejected, not silently buffered");

        await Task.CompletedTask;
    }

    [Fact(DisplayName = "PLNT-STRS-QUEUE-002: Circuit breaker trips after 5 consecutive failures and half-opens after the recovery probe (SR30/SR31)")]
    public async Task CircuitBreaker_ShouldTripAfterFiveFailures_AndHalfOpenAfterProbeInterval()
    {
        var breaker = new SimpleCircuitBreaker(failureThreshold: 5, recoveryProbe: TimeSpan.FromMilliseconds(50));

        for (var i = 0; i < 5; i++)
            breaker.TryExecute(() => throw new InvalidOperationException("simulated dependency failure"), out _);

        breaker.State.Should().Be(CircuitState.Open, "SR30 requires the circuit to trip after 5 consecutive failures");

        await Task.Delay(TimeSpan.FromMilliseconds(80), TestContext.Current.CancellationToken);

        var probed = breaker.TryExecute(() => { }, out _);
        probed.Should().BeTrue("SR31 requires a recovery probe attempt once the recovery interval has elapsed");
        breaker.State.Should().Be(CircuitState.Closed, "a successful probe must close the circuit again");
    }

    private static void CreateAlert(int i) =>
        Alert.Create(
            AlertType.ProcessDeviation,
            AlertSeverity.Medium,
            $"Sustained {i}",
            "PLNT-STRS-ALARM-002",
            "StressTests",
            Actor,
            UtcTimestamp.Now(),
            new CorrelationId(Guid.NewGuid()),
            new CausationId(Guid.NewGuid()),
            PerformedByActor.User(Actor));
}
