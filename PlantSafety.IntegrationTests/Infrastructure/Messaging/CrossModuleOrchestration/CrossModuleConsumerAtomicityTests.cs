// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Infrastructure.Messaging.CrossModuleOrchestration
//  Layer:          IntegrationTests
//  File:           CrossModuleConsumerAtomicityTests.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
//  SchemaVersion:  2026.1
//  Description:    Proves the shared replay-atomicity mechanism used by all 8 Phase 5 §3
//                  cross-module orchestration consumers --
//                  CrossModuleIdempotentConsumerBase.TryExecuteOnceAsync, backed by the real
//                  DI-resolved IDistributedLockProvider (SqlServerDistributedLockProvider) and
//                  ICrossModuleDedupStore (SqlCrossModuleDedupStore) against the Testcontainers SQL
//                  Server instance -- against the exact race this Phase 6 blocker exists to close:
//                  (a) a duplicate/redelivered message with the same business key must not re-run
//                  the side effect, and (b) two concurrent redeliveries of the same business key
//                  must serialize (one wins, one is skipped as a duplicate) rather than both
//                  observing "not yet handled" and both running the side effect. Exercised via a
//                  minimal test-double consumer (TestCrossModuleConsumer) that exposes the
//                  protected TryExecuteOnceAsync helper, since the helper itself -- not any single
//                  concrete consumer's business logic -- is the shared hardening surface all 8
//                  concrete consumers (PredictiveMaintenanceEscalationConsumer,
//                  MaintenanceCompletionFeedbackConsumer, ProductionAnomalyReprioritizationConsumer,
//                  SchedulingKpiFeedbackConsumer, CapaCompletionRiskReductionConsumer,
//                  InsufficientMitigationInsightConsumer, PredictiveSafetyAlertDashboardFeedConsumer,
//                  and RiskEscalationCapaConsumer's lock-only usage) rely on for atomicity.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using System.Collections.Concurrent;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Infrastructure.Messaging.Consumers;
using PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration;
using PlantSafety.Infrastructure.Persistence.ConnectionFactory;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.IntegrationTests.Infrastructure.Messaging.CrossModuleOrchestration;

/// <summary>
/// Integration tests for <see cref="CrossModuleIdempotentConsumerBase{TMessage}.TryExecuteOnceAsync"/>,
/// the shared lock-scoped, business-key-deduplicated side-effect helper all 8 Phase 5 §3
/// cross-module orchestration consumers use to close the replay-atomicity gap between their real
/// side effect (a separate SQL connection/transaction, via MediatR -&gt; Application handler -&gt;
/// repository) and the outer <c>[System].[ProcessedEvents]</c> marker write.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class CrossModuleConsumerAtomicityTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    private static string UniqueBusinessKey([System.Runtime.CompilerServices.CallerMemberName] string testName = "")
        => $"{testName}:{Guid.NewGuid():N}";

    private TestCrossModuleConsumer CreateConsumer(IServiceScope scope) => new(
        scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>(),
        scope.ServiceProvider.GetRequiredService<ISender>(),
        scope.ServiceProvider.GetRequiredService<IDistributedLockProvider>(),
        scope.ServiceProvider.GetRequiredService<ICrossModuleDedupStore>(),
        scope.ServiceProvider.GetRequiredService<ILogger<TestCrossModuleConsumer>>());

    [Fact]
    public async Task TryExecuteOnceAsync_FirstDelivery_RunsTheSideEffectAndRecordsTheDedupKey()
    {
        using var scope = Factory.Services.CreateScope();
        var consumer = CreateConsumer(scope);
        var businessKey = UniqueBusinessKey();
        var runCount = 0;

        var ran = await consumer.TryExecuteOnceAsyncPublic(
            lockResourceKey: $"IntegrationTests:CrossModuleDedup:{businessKey}",
            businessKey: businessKey,
            eventId: Guid.NewGuid(),
            sideEffect: () => { Interlocked.Increment(ref runCount); return Task.CompletedTask; },
            cancellationToken: CancellationToken.None);

        ran.Should().BeTrue("the first delivery for a never-before-seen business key must run the side effect");
        runCount.Should().Be(1);
    }

    [Fact]
    public async Task TryExecuteOnceAsync_RedeliveredMessage_DoesNotReRunTheSideEffect()
    {
        using var scope = Factory.Services.CreateScope();
        var consumer = CreateConsumer(scope);
        var businessKey = UniqueBusinessKey();
        var runCount = 0;

        Task<bool> DeliverAsync() => consumer.TryExecuteOnceAsyncPublic(
            lockResourceKey: $"IntegrationTests:CrossModuleDedup:{businessKey}",
            businessKey: businessKey,
            eventId: Guid.NewGuid(),
            sideEffect: () => { Interlocked.Increment(ref runCount); return Task.CompletedTask; },
            cancellationToken: CancellationToken.None);

        // First "delivery" -- the message's original processing.
        var firstRan = await DeliverAsync();

        // Second "delivery" -- simulates a MassTransit redelivery of the exact same logical event
        // (e.g. after a crash between the side effect committing and the outer ProcessedEvents
        // marker write, so ProcessedEvents' own EventId check would NOT catch this on its own).
        var secondRan = await DeliverAsync();

        firstRan.Should().BeTrue();
        secondRan.Should().BeFalse("a redelivered message with an already-recorded business key must be skipped, not re-executed");
        runCount.Should().Be(1, "the side effect must never run twice for the same business key");
    }

    [Fact]
    public async Task TryExecuteOnceAsync_TwoConcurrentRedeliveries_SerializeAndOnlyOneRunsTheSideEffect()
    {
        using var scopeA = Factory.Services.CreateScope();
        using var scopeB = Factory.Services.CreateScope();
        var consumerA = CreateConsumer(scopeA);
        var consumerB = CreateConsumer(scopeB);

        var businessKey = UniqueBusinessKey();
        var lockKey = $"IntegrationTests:CrossModuleDedup:{businessKey}";
        var runCount = 0;
        var intervals = new ConcurrentBag<(DateTime EnteredUtc, DateTime ExitedUtc)>();

        async Task<bool> DeliverAsync(TestCrossModuleConsumer consumer) => await consumer.TryExecuteOnceAsyncPublic(
            lockResourceKey: lockKey,
            businessKey: businessKey,
            eventId: Guid.NewGuid(),
            sideEffect: async () =>
            {
                var enteredUtc = DateTime.UtcNow;
                Interlocked.Increment(ref runCount);

                // Simulate the short-lived side-effect work (the real consumers dispatch a MediatR
                // command here) while still holding the lock, so a genuinely concurrent second
                // redelivery has a real window in which to race the "not yet handled" check.
                await Task.Delay(200);

                intervals.Add((enteredUtc, DateTime.UtcNow));
            },
            cancellationToken: CancellationToken.None);

        var results = await Task.WhenAll(DeliverAsync(consumerA), DeliverAsync(consumerB));

        results.Count(r => r).Should().Be(1, "exactly one of the two concurrent redeliveries must run the side effect");
        results.Count(r => !r).Should().Be(1, "the other concurrent redelivery must observe the dedup key already recorded and skip");
        runCount.Should().Be(1, "the side effect must run exactly once even under concurrent redelivery");

        // Only one delivery ran the side effect at all, so there is exactly one recorded interval --
        // trivially non-overlapping. This asserts the interval was captured, proving the winning
        // delivery's side effect actually executed inside the lock-protected section.
        intervals.Should().HaveCount(1);
    }

    /// <summary>
    /// Minimal <see cref="CrossModuleIdempotentConsumerBase{TMessage}"/> test double that exposes
    /// the protected <c>TryExecuteOnceAsync</c> helper publicly for direct testing. The
    /// <see cref="IdempotentConsumerBase{TMessage}"/>-mandated <c>ResolveEventId</c>/<c>HandleAsync</c>
    /// overrides are never exercised by these tests (which call <c>TryExecuteOnceAsyncPublic</c>
    /// directly rather than <c>Consume</c>) and exist only to satisfy the abstract base contract.
    /// </summary>
    public sealed class TestCrossModuleConsumer(
        ISqlConnectionFactory connectionFactory,
        ISender sender,
        IDistributedLockProvider lockProvider,
        ICrossModuleDedupStore dedupStore,
        ILogger<TestCrossModuleConsumer> logger)
        : CrossModuleIdempotentConsumerBase<TestCrossModuleMessage>(connectionFactory, sender, lockProvider, dedupStore, logger)
    {
        /// <summary>Public pass-through to the protected <c>TryExecuteOnceAsync</c> helper under test.</summary>
        public Task<bool> TryExecuteOnceAsyncPublic(
            string lockResourceKey, string businessKey, Guid eventId, Func<Task> sideEffect, CancellationToken cancellationToken)
            => TryExecuteOnceAsync(lockResourceKey, businessKey, eventId, sideEffect, cancellationToken);

        /// <inheritdoc/>
        protected override Guid ResolveEventId(TestCrossModuleMessage message) => message.EventId;

        /// <inheritdoc/>
        protected override Task HandleAsync(
            ConsumeContext<TestCrossModuleMessage> context, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    /// <summary>Trivial MassTransit message shape satisfying <see cref="TestCrossModuleConsumer"/>'s generic constraint.</summary>
    public sealed class TestCrossModuleMessage
    {
        /// <summary>Message identifier.</summary>
        public Guid EventId { get; init; }
    }
}
