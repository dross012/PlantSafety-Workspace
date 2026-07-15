// ============================================================================
//  Test IDs:  PLNT-PERF-ALARM-001..003
//  Manuals:   tests_rules_design_manual.md Ch.9 ("Alarm response time: <=500ms
//             at p99 under nominal load (Critical defect if exceeded)");
//             performance_stress_manual.md (P95<=250ms/P99<=500ms thresholds,
//             async-first, deterministic, no Thread.Sleep in measured path).
//  Purpose:   Measures in-process latency of the Alert aggregate's creation
//             path (the pure-domain, DB-independent portion of the "alarm
//             response" pipeline: validation + invariant enforcement + domain
//             event emission). End-to-end (network + persistence) alarm
//             latency is covered separately by PlantSafety.IntegrationTests /
//             PlantSafety.ScenarioTests against a real database.
// ============================================================================
namespace PlantSafety.PerformanceTests.Alerts;

public sealed class AlarmResponsePerformanceTests
{
    private static readonly UserId Actor = UserId.FromGuid(Guid.NewGuid());
    private const int SampleSize = 2_000;

    [Fact(DisplayName = "PLNT-PERF-ALARM-001: Alert.Create p99 latency <= 500ms (hard gate, Critical defect if exceeded)")]
    public void AlertCreate_P99Latency_ShouldBeAtMost500Milliseconds()
    {
        var samples = new List<double>(SampleSize);

        for (var i = 0; i < SampleSize; i++)
        {
            var sw = Stopwatch.StartNew();

            Alert.Create(
                AlertType.WorkerSafety,
                AlertSeverity.Emergency,
                $"Perf alarm {i}",
                "PLNT-PERF-ALARM-001 sample",
                "PerformanceTests",
                Actor,
                UtcTimestamp.Now(),
                new CorrelationId(Guid.NewGuid()),
                new CausationId(Guid.NewGuid()),
                PerformedByActor.User(Actor));

            sw.Stop();
            samples.Add(sw.Elapsed.TotalMilliseconds);
        }

        var p99 = Percentile(samples, 0.99);
        var p95 = Percentile(samples, 0.95);

        p95.Should().BeLessThanOrEqualTo(250,
            "performance_stress_manual.md requires P95 <= 250ms for the safety-critical alarm path");
        p99.Should().BeLessThanOrEqualTo(500,
            "tests_rules_design_manual.md Ch.9 requires alarm response <= 500ms at p99 (Critical defect if exceeded)");
    }

    [Fact(DisplayName = "PLNT-PERF-ALARM-002: Interlock-equivalent (Emergency severity) trigger completes within 100ms per scenario, single-shot")]
    public void EmergencyAlertCreate_SingleShotLatency_ShouldBeAtMost100Milliseconds()
    {
        // Proxy for the "interlock trigger fires within 100ms" hard number: the domain
        // layer has no literal Interlock aggregate, so Emergency-severity Alert creation
        // (the highest-priority safety trigger path in this codebase) is used as the
        // closest analogue for the sub-100ms hard-real-time requirement.
        var sw = Stopwatch.StartNew();

        var alert = Alert.Create(
            AlertType.WorkerSafety,
            AlertSeverity.Emergency,
            "Interlock-equivalent trigger",
            "PLNT-PERF-ALARM-002",
            "PerformanceTests",
            Actor,
            UtcTimestamp.Now(),
            new CorrelationId(Guid.NewGuid()),
            new CausationId(Guid.NewGuid()),
            PerformedByActor.User(Actor));

        sw.Stop();

        alert.Should().NotBeNull();
        sw.Elapsed.TotalMilliseconds.Should().BeLessThanOrEqualTo(100,
            "tests_rules_design_manual.md Ch.9 requires interlock trigger within 100ms");
    }

    [Fact(DisplayName = "PLNT-PERF-ALARM-003: Zero false negatives across 10,000 Emergency-alert trigger scenarios")]
    public void EmergencyAlertCreate_Across10000Scenarios_ShouldNeverFailToTrigger()
    {
        const int scenarioCount = 10_000;
        var falseNegatives = 0;

        for (var i = 0; i < scenarioCount; i++)
        {
            try
            {
                var alert = Alert.Create(
                    AlertType.WorkerSafety,
                    AlertSeverity.Emergency,
                    $"Scenario {i}",
                    "PLNT-PERF-ALARM-003",
                    "PerformanceTests",
                    Actor,
                    UtcTimestamp.Now(),
                    new CorrelationId(Guid.NewGuid()),
                    new CausationId(Guid.NewGuid()),
                    PerformedByActor.User(Actor));

                if (alert.Severity != AlertSeverity.Emergency)
                    falseNegatives++;
            }
            catch
            {
                falseNegatives++;
            }
        }

        falseNegatives.Should().Be(0,
            "tests_rules_design_manual.md Ch.9 requires zero false negatives per 10,000 interlock-equivalent scenarios");
    }

    private static double Percentile(List<double> samples, double percentile)
    {
        var sorted = samples.OrderBy(x => x).ToList();
        var index = (int)Math.Ceiling(percentile * sorted.Count) - 1;
        index = Math.Clamp(index, 0, sorted.Count - 1);
        return sorted[index];
    }
}
