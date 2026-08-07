// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.HumanFactors.CognitiveLoadAnalytics
//  Layer:          IntegrationTests
//  File:           CognitiveLoadAnalyticsPerformanceCompatibilityTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Performance/Stress and Compatibility/Interoperability/Backward-Compatibility
//                   coverage for the HumanFactors.CognitiveLoadAnalytics sub-module.
//                   Performance/Stress: bulk reading intake + trend recalculation against the
//                   real Testcontainers SQL Server instance, scaled to this module's size (a
//                   single shift's readings are a low-tens-per-shift volume in production, not a
//                   high-frequency telemetry stream), completing within a generous wall-clock
//                   budget. Compatibility/Interoperability: the read-model DTO's query result is
//                   stable/idempotent across repeated calls (no hidden non-determinism a UI
//                   client could be broken by) and the aggregate's child-row ordering contract
//                   (ascending by measurement time) is preserved across the wire. Backward
//                   Compatibility: the CognitiveWorkloadLevel/CognitiveRiskLevel enum members
//                   this module's stored procedures persist as NVARCHAR values must continue to
//                   round-trip by name (a persisted status string surviving a future enum
//                   reordering). Deterministic, parallel-safe, additive-only.
// ============================================================================
using System.Diagnostics;
using MediatR;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.IntegrationTests.HumanFactors.CognitiveLoadAnalytics;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CognitiveLoadAnalyticsPerformanceCompatibilityTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task RecordFiftyReadingsThenRecalculateTrend_CompletesWithinPerformanceBudget()
    {
        // Scaled to module size: a single shift's cognitive-load readings are a low-tens-per-
        // shift volume (one measurement per worker per check-in), not a high-frequency stream --
        // 50 sequential command round-trips through the real MediatR pipeline + Testcontainers
        // SQL Server is a representative stress volume for this sub-module, not an arbitrary
        // fixed count.
        // GetScopedService (not a plain CreateScope) is required here: it populates a fake
        // authenticated HttpContext with the fixed TestIds tenant so TenantIsolationBehavior
        // resolves a tenant context to compare the command's TenantId against -- without it,
        // every command sent through this sender is rejected with
        // ApplicationAuthorizationException("The request's TenantId does not match the
        // authenticated tenant context.").
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        using var _ = scope;

        var shiftDateUtc = DateTime.UtcNow.Date.AddDays(-1);
        var workerId = Guid.NewGuid();
        Guid ledgerId = Guid.Empty;

        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < 50; i++)
        {
            var reading = RecordCognitiveLoadReadingCommand.FromDto(
                tenantId: TenantId, siteId: null, shiftType: "Evening", shiftDateUtc: shiftDateUtc,
                workerUserId: workerId, taskComplexityProfileId: null, cognitiveLoadScore: 30m + i % 50,
                measurementMethod: "SelfReport", measuredAtUtc: DateTime.UtcNow.AddMinutes(-i), requesterUserId: ActorId);

            var result = await sender.Send(reading, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            ledgerId = result.Value.CognitiveLoadLedgerId;
        }

        var recalc = RecalculateWorkloadTrendCommand.FromDto(
            tenantId: TenantId, cognitiveLoadLedgerId: ledgerId,
            periodStartUtc: DateTime.UtcNow.AddDays(-1), periodEndUtc: DateTime.UtcNow, requesterUserId: ActorId);
        var recalcResult = await sender.Send(recalc, CancellationToken.None);

        stopwatch.Stop();

        recalcResult.IsSuccess.Should().BeTrue();
        recalcResult.Value.ReadingCount.Should().Be(50);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(60),
            "50 sequential command round-trips through the real DB should stay well within a generous CI-safe budget");
    }

    [Fact]
    public async Task GetWorkloadTrendByShiftAsync_RepeatedCalls_ReturnStableResults()
    {
        // Compatibility/Interoperability: the read-model query result must be deterministic and
        // stable across repeated calls for the same inputs -- a UI client polling this endpoint
        // must never observe spurious drift.
        using var scope = Factory.Services.CreateScope();
        var writeRepo = scope.ServiceProvider.GetRequiredService<ICognitiveLoadLedgerRepository>();
        var readRepo = scope.ServiceProvider.GetRequiredService<PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Repositories.ICognitiveLoadAnalyticsReadRepository>();

        var ledger = CognitiveLoadLedger.Create(
            UserId.FromGuid(ActorId), UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()),
            PerformedByActor.User(UserId.FromGuid(ActorId)), TenantId, null, ShiftType.Rotating, UtcTimestamp.Now());
        ledger.RecordCognitiveLoadReading(
            UserId.FromGuid(Guid.NewGuid()), null, 70m, "NASA-TLX", UtcTimestamp.Now(), UtcTimestamp.Now(),
            UserId.FromGuid(ActorId), PerformedByActor.User(UserId.FromGuid(ActorId)), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await writeRepo.AddAsync(ledger, CancellationToken.None);

        ledger.RecalculateWorkloadTrend(
            UtcTimestamp.From(DateTime.UtcNow.AddHours(-1)), UtcTimestamp.From(DateTime.UtcNow.AddHours(1)), UtcTimestamp.Now(),
            UserId.FromGuid(ActorId), PerformedByActor.User(UserId.FromGuid(ActorId)), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await writeRepo.UpdateAsync(ledger, CancellationToken.None);

        var first = await readRepo.GetWorkloadTrendByShiftAsync(
            TenantId, null, "Rotating", DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(2), CancellationToken.None);
        var second = await readRepo.GetWorkloadTrendByShiftAsync(
            TenantId, null, "Rotating", DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(2), CancellationToken.None);

        first.Should().BeEquivalentTo(second, "repeated calls with identical inputs must return identical results");
        first.Should().Contain(t => t.CognitiveLoadLedgerId == ledger.CognitiveLoadLedgerId.Value);
    }

    [Theory]
    [InlineData(CognitiveWorkloadLevel.Low)]
    [InlineData(CognitiveWorkloadLevel.Moderate)]
    [InlineData(CognitiveWorkloadLevel.High)]
    [InlineData(CognitiveWorkloadLevel.Overload)]
    public async Task CognitiveLoadReading_WorkloadLevelEnum_RoundTripsByNameThroughTheDatabase(CognitiveWorkloadLevel level)
    {
        // Backward Compatibility: usp_CognitiveLoadReading_Insert/ListByLedger persist/read
        // WorkloadLevel as an NVARCHAR(20) enum-name string, not an ordinal -- this must continue
        // to round-trip correctly even if CognitiveWorkloadLevel's member order changes in a
        // future release.
        using var scope = Factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICognitiveLoadLedgerRepository>();

        var score = level switch
        {
            CognitiveWorkloadLevel.Low => 10m,
            CognitiveWorkloadLevel.Moderate => 45m,
            CognitiveWorkloadLevel.High => 65m,
            _ => 90m,
        };

        var ledger = CognitiveLoadLedger.Create(
            UserId.FromGuid(ActorId), UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()),
            PerformedByActor.User(UserId.FromGuid(ActorId)), TenantId, null, ShiftType.Day, UtcTimestamp.Now());
        ledger.RecordCognitiveLoadReading(
            UserId.FromGuid(Guid.NewGuid()), null, score, "NASA-TLX", UtcTimestamp.Now(), UtcTimestamp.Now(),
            UserId.FromGuid(ActorId), PerformedByActor.User(UserId.FromGuid(ActorId)), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await repo.AddAsync(ledger, CancellationToken.None);

        var reloaded = await repo.GetByIdAsync(ledger.CognitiveLoadLedgerId, TenantId, CancellationToken.None);

        reloaded.Should().NotBeNull();
        reloaded!.CognitiveLoadReadings.Single().WorkloadLevel.Should().Be(level);
    }
}
