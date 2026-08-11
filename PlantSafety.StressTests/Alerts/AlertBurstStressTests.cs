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

    /// <summary>
    /// Asserts Alert.Create does not degrade under concurrency.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This deliberately does NOT assert PR10's "&gt;=2x expected production throughput". That is an
    /// absolute floor against a documented production rate, and no such rate exists anywhere in
    /// this repository -- performance_stress_manual.md is cited by these tests but is not present.
    /// Inventing a number would produce a threshold that looks authoritative and means nothing, so
    /// the absolute check is left unwritten and called out rather than faked. See the note below.
    /// </para>
    /// <para>
    /// What this replaces was broken three ways. It timed a cold, un-warmed sequential loop as the
    /// baseline, so the first iterations paid JIT costs. It then compared that to a parallel run
    /// over the same trivially cheap operation, where Parallel.ForEachAsync's per-item scheduling
    /// overhead is on the same order as Alert.Create itself -- so the ratio sat at roughly 1.0 and
    /// the test passed or failed on scheduler noise (observed: fail, pass, fail across three
    /// consecutive isolated runs, with the computed threshold itself moving between runs because
    /// it was derived from the same run's baseline). And despite its name and message both saying
    /// "2x", it asserted `sustained >= baseline`, i.e. 1x: the `* 2` was on the operation COUNT,
    /// not the throughput multiplier.
    /// </para>
    /// <para>
    /// The property that IS meaningful and stable here is that going concurrent does not make
    /// throughput collapse -- which is what would happen if Alert.Create grew a lock, shared
    /// mutable state, or contended allocation. A generous floor is used because parallel speedup
    /// is not the claim; only the absence of serialisation is.
    /// </para>
    /// </remarks>
    [Fact(DisplayName = "PLNT-STRS-ALARM-002: Concurrent Alert.Create throughput does not collapse against sequential")]
    public async Task AlertCreate_UnderConcurrency_ShouldNotCollapseAgainstSequentialThroughput()
    {
        const int warmupOps = 2_000;
        const int measuredOps = 20_000;

        // Warm up so neither measurement pays JIT or first-allocation costs.
        for (var i = 0; i < warmupOps; i++)
            CreateAlert(i);

        await Parallel.ForEachAsync(Enumerable.Range(0, warmupOps),
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            (i, _) => { CreateAlert(i); return ValueTask.CompletedTask; });

        var sequentialSw = Stopwatch.StartNew();
        for (var i = 0; i < measuredOps; i++)
            CreateAlert(i);
        sequentialSw.Stop();
        var sequentialOpsPerSecond = measuredOps / sequentialSw.Elapsed.TotalSeconds;

        var concurrentSw = Stopwatch.StartNew();
        await Parallel.ForEachAsync(Enumerable.Range(0, measuredOps),
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            (i, _) => { CreateAlert(i); return ValueTask.CompletedTask; });
        concurrentSw.Stop();
        var concurrentOpsPerSecond = measuredOps / concurrentSw.Elapsed.TotalSeconds;

        // A quarter of sequential throughput. Well below anything a healthy path produces, and far
        // enough from the ~1.0 ratio that scheduling noise cannot reach it, while still failing
        // hard if Alert.Create ever serialises under contention.
        const double collapseFloor = 0.25;

        concurrentOpsPerSecond.Should().BeGreaterThan(sequentialOpsPerSecond * collapseFloor,
            "Alert.Create must not serialise under concurrency: {0:N0} ops/s concurrent vs {1:N0} ops/s sequential " +
            "is a {2:P0} ratio, below the {3:P0} floor, which indicates lock contention or shared mutable state " +
            "on the safety-critical alert path",
            concurrentOpsPerSecond, sequentialOpsPerSecond,
            concurrentOpsPerSecond / sequentialOpsPerSecond, collapseFloor);
    }

    // NOTE (PR10, unimplemented on purpose): the manual's "sustain >=2x expected production
    // throughput" needs a documented expected production rate for Alert.Create -- e.g. "alerts/sec
    // at peak across all tenants". Once that number exists, add a test asserting an absolute floor
    // against it. It cannot be derived from a same-run baseline: comparing a machine to itself
    // says nothing about whether it meets production demand.

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
