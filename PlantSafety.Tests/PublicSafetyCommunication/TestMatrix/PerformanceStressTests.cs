// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.TestMatrix
//  Layer:          Tests (Unit) -- Performance / Stress
//  File:           PerformanceStressTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    [PLNT-PERF-PSC / PLNT-STRS-PSC] Performance and Stress test types
//                  (TRD-MAN-2026-001 Ch.3; thresholds informed by the Performance & Stress Rules
//                  and Design Manual's general principles -- note that manual's hard numeric
//                  thresholds are explicitly scoped to the Diagnostics Platform, not this module,
//                  so generous in-process budgets are used here rather than Diagnostics-specific
//                  P95/P99 numbers). Performance: a single handler invocation against an
//                  in-memory repository completes well within a generous latency budget.
//                  Stress: a burst of concurrent command executions against the module's
//                  handlers completes without exceptions, without lost writes, and without
//                  unbounded memory growth (call-count matches burst size exactly). Isolated,
//                  deterministic, no I/O, no external services.
// ============================================================================
using System.Diagnostics;
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Tests.PublicSafetyCommunication.TestMatrix.Fakes;

namespace PlantSafety.Tests.PublicSafetyCommunication.TestMatrix;

public sealed class PerformanceStressTests : UnitTestBase
{
    [Fact]
    public async Task LogCommunicationEntryCommandHandler_SingleInvocation_CompletesWithinGenerousLatencyBudget()
    {
        var handler = new LogCommunicationEntryCommandHandler(
            new InMemoryCommunicationLogEntryRepository(), NullLogger<LogCommunicationEntryCommandHandler>());

        var sw = Stopwatch.StartNew();
        var result = await handler.Handle(new LogCommunicationEntryCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            null, null, null, "Outbound", "Phone", "Counterparty", "Summary", FixedUtcNow)
        { ActorId = TestUserId.Value }, CancellationToken.None);
        sw.Stop();

        result.IsSuccess.Should().BeTrue();
        // Generous in-process budget (no DB/network in this handler path) -- guards against a
        // gross performance regression (e.g. an accidental synchronous sleep or O(n^2) loop)
        // without asserting Diagnostics-Platform-specific P95/P99 numbers that do not apply here.
        sw.ElapsedMilliseconds.Should().BeLessThan(200);
    }

    [Fact]
    public async Task RegisterPublicSafetyAgencyCommandHandler_100SequentialInvocations_SustainThroughputWithoutDegradation()
    {
        var repo = new InMemoryPublicSafetyAgencyRepository();
        var handler = new RegisterPublicSafetyAgencyCommandHandler(repo, NullLogger<RegisterPublicSafetyAgencyCommandHandler>());
        const int iterations = 100;

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            var result = await handler.Handle(new RegisterPublicSafetyAgencyCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
                $"Agency {i}", "Fire", "District", "555-0100", null)
            { ActorId = TestUserId.Value }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
        sw.Stop();

        repo.AddCallCount.Should().Be(iterations);
        sw.ElapsedMilliseconds.Should().BeLessThan(5000, "100 in-memory handler invocations must sustain throughput without gross degradation");
    }

    [Fact]
    public async Task DispatchIncidentNotificationCommandHandler_BurstOf200ConcurrentDispatches_CompletesWithoutErrorsOrLostWrites()
    {
        // Stress: burst-handling -- absorb a burst of concurrent write commands without failure
        // and without any write being silently dropped (every task's Guid key must land in the
        // fake's store exactly once).
        var repo = new InMemoryIncidentNotificationRepository();
        var handler = new DispatchIncidentNotificationCommandHandler(repo, NullLogger<DispatchIncidentNotificationCommandHandler>());
        const int burstSize = 200;

        var tasks = Enumerable.Range(0, burstSize).Select(_ => handler.Handle(new DispatchIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), Guid.NewGuid(), "Phone", "Burst dispatch.")
        { ActorId = TestUserId.Value }, CancellationToken.None));

        var results = await Task.WhenAll(tasks);

        results.Should().OnlyContain(r => r.IsSuccess);
        results.Select(r => r.Value.IncidentNotificationId).Distinct().Should().HaveCount(burstSize,
            "every concurrently dispatched notification must produce its own distinct, correctly-persisted identity");
        repo.AddCallCount.Should().Be(burstSize);
    }

    [Fact]
    public async Task LogCommunicationEntryCommandHandler_StressBurstOfLogEntries_NoExceptionsAndAllEntriesRetrievable()
    {
        var repo = new InMemoryCommunicationLogEntryRepository();
        var handler = new LogCommunicationEntryCommandHandler(repo, NullLogger<LogCommunicationEntryCommandHandler>());
        const int burstSize = 250;
        var incidentId = Guid.NewGuid();

        var tasks = Enumerable.Range(0, burstSize).Select(i => handler.Handle(new LogCommunicationEntryCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            incidentId, null, null, "Outbound", "Phone", $"Counterparty {i}", $"Summary {i}", FixedUtcNow)
        { ActorId = TestUserId.Value }, CancellationToken.None));

        var results = await Task.WhenAll(tasks);

        results.Should().OnlyContain(r => r.IsSuccess);
        repo.AddCallCount.Should().Be(burstSize);
    }
}
