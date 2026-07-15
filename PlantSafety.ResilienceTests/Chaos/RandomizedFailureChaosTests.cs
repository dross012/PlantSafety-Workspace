// ============================================================================
//  Test IDs:  PLNT-CHAO-ALERT-001..002
//  Manual:    tests_rules_design_manual.md (CHAO type; tooling: Azure Chaos
//             Studio for production, deterministic seeded-random injection
//             for unit-scope chaos per Foundational Principles).
//  Purpose:   Injects randomized (seeded, deterministic) transient failures
//             into a batch of safety-critical Alert.Create operations and
//             confirms the system degrades gracefully: failures are isolated
//             per-operation (no cascading failure across the batch), and the
//             success/failure count is fully accounted for (no silent drops).
// ============================================================================
namespace PlantSafety.ResilienceTests.Chaos;

public sealed class RandomizedFailureChaosTests
{
    private static readonly UserId Actor = UserId.FromGuid(Guid.NewGuid());

    [Fact(DisplayName = "PLNT-CHAO-ALERT-001: Randomized invalid-input chaos (seeded) never crashes the batch, only isolates individual failures")]
    public void ChaosInjectedBatch_ShouldIsolateFailures_WithoutCascading()
    {
        var random = new Random(Seed: 42); // fixed seed => deterministic per manual's Foundational Principles
        const int batchSize = 5_000;
        var succeeded = 0;
        var failed = 0;

        for (var i = 0; i < batchSize; i++)
        {
            // ~10% chaos-injected invalid title (blank), which Alert.Create's own guard clause
            // is expected to reject deterministically rather than corrupting shared state.
            var chaosInjected = random.NextDouble() < 0.10;
            var title = chaosInjected ? string.Empty : $"Chaos alert {i}";

            try
            {
                Alert.Create(
                    AlertType.WorkerSafety,
                    AlertSeverity.High,
                    title,
                    "PLNT-CHAO-ALERT-001",
                    "ResilienceTests",
                    Actor,
                    UtcTimestamp.Now(),
                    new CorrelationId(Guid.NewGuid()),
                    new CausationId(Guid.NewGuid()),
                    PerformedByActor.User(Actor));
                succeeded++;
            }
            catch (ArgumentException)
            {
                failed++;
            }
        }

        (succeeded + failed).Should().Be(batchSize, "every operation must be accounted for -- no silent drops under chaos");
        failed.Should().BeGreaterThan(0, "the chaos injection must actually have exercised the failure path");
        succeeded.Should().BeGreaterThan(0, "non-chaos-injected operations must continue to succeed despite nearby failures (no cascading failure)");
    }

    [Fact(DisplayName = "PLNT-CHAO-ALERT-002: Concurrent chaos-injected failures do not corrupt unrelated concurrent successes")]
    public async Task ConcurrentChaosInjection_ShouldNotCorruptUnrelatedOperations()
    {
        var results = new System.Collections.Concurrent.ConcurrentBag<bool>();

        await Parallel.ForEachAsync(Enumerable.Range(0, 2_000), (i, _) =>
        {
            var chaosInjected = i % 7 == 0; // deterministic chaos pattern, not real randomness, for reproducibility
            try
            {
                var alert = Alert.Create(
                    AlertType.MachineFailure,
                    AlertSeverity.Medium,
                    chaosInjected ? string.Empty : $"Concurrent {i}",
                    "PLNT-CHAO-ALERT-002",
                    "ResilienceTests",
                    Actor,
                    UtcTimestamp.Now(),
                    new CorrelationId(Guid.NewGuid()),
                    new CausationId(Guid.NewGuid()),
                    PerformedByActor.User(Actor));
                results.Add(alert.Title == $"Concurrent {i}");
            }
            catch (ArgumentException)
            {
                results.Add(!chaosInjected ? false : true); // only chaos-injected iterations may fail
            }
            return ValueTask.CompletedTask;
        });

        results.Should().OnlyContain(r => r, "no non-chaos-injected operation should have failed or produced a mismatched result");
    }
}
