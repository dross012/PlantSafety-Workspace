// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.HumanFactors.CognitiveLoadAnalytics
//  Layer:          IntegrationTests
//  File:           CognitiveLoadAnalyticsSecurityResilienceTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Security, Resilience, Chaos, and Failover/Recovery coverage for the
//                   HumanFactors.CognitiveLoadAnalytics sub-module, resolving the real Dapper
//                   repositories from the DI container against the Testcontainers SQL Server
//                   instance. Security: individually identifiable worker cognitive-load data
//                   must stay tenant-isolated at the stored-procedure query filter (data
//                   governance requirement per the module brief). Resilience/Chaos: malformed
//                   command input and repeated child-record persistence must never corrupt the
//                   ledger or throw unhandled exceptions. Failover/Recovery: re-submitting an
//                   aggregate's full child collection (the repository's AddAsync/UpdateAsync
//                   precedent for idempotent retries) must not duplicate rows.
//                   Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.IntegrationTests.HumanFactors.CognitiveLoadAnalytics;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CognitiveLoadAnalyticsSecurityResilienceTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetByIdAsync_WhenLedgerBelongsToDifferentTenant_ReturnsNullRatherThanLeakingSensitiveData()
    {
        using var scope = Factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICognitiveLoadLedgerRepository>();

        var owningTenantId = TenantId;
        var otherTenantId = Guid.NewGuid();

        var ledger = CognitiveLoadLedger.Create(
            UserId.FromGuid(ActorId), UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()),
            PerformedByActor.User(UserId.FromGuid(ActorId)), owningTenantId, null, ShiftType.Day, UtcTimestamp.Now());
        ledger.RecordCognitiveLoadReading(
            UserId.FromGuid(Guid.NewGuid()), null, 90m, "NASA-TLX", UtcTimestamp.Now(), UtcTimestamp.Now(),
            UserId.FromGuid(ActorId), PerformedByActor.User(UserId.FromGuid(ActorId)), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await repo.AddAsync(ledger, CancellationToken.None);

        var crossTenantLookup = await repo.GetByIdAsync(ledger.CognitiveLoadLedgerId, otherTenantId, CancellationToken.None);

        crossTenantLookup.Should().BeNull(
            "the usp_CognitiveLoadLedger_GetById stored procedure filters by TenantId -- individually identifiable " +
            "worker cognitive-load data must never be retrievable across a tenant boundary");
    }

    [Fact]
    public async Task GetCognitiveLoadByWorkerAsync_ScopesStrictlyByTenantId()
    {
        using var scope = Factory.Services.CreateScope();
        var writeRepo = scope.ServiceProvider.GetRequiredService<ICognitiveLoadLedgerRepository>();
        var readRepo = scope.ServiceProvider.GetRequiredService<PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Repositories.ICognitiveLoadAnalyticsReadRepository>();

        var workerId = Guid.NewGuid();
        var ledger = CognitiveLoadLedger.Create(
            UserId.FromGuid(ActorId), UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()),
            PerformedByActor.User(UserId.FromGuid(ActorId)), TenantId, null, ShiftType.Day, UtcTimestamp.Now());
        ledger.RecordCognitiveLoadReading(
            UserId.FromGuid(workerId), null, 55m, "NASA-TLX", UtcTimestamp.Now(), UtcTimestamp.Now(),
            UserId.FromGuid(ActorId), PerformedByActor.User(UserId.FromGuid(ActorId)), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await writeRepo.AddAsync(ledger, CancellationToken.None);

        var otherTenantResult = await readRepo.GetCognitiveLoadByWorkerAsync(
            Guid.NewGuid(), workerId, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1), CancellationToken.None);

        otherTenantResult.Should().BeNull("a worker's cognitive-load rollup must not be visible from a query scoped to a different tenant");
    }

    [Fact]
    public async Task RecordCognitiveLoadReadingCommandHandler_WithUnrecognizedShiftType_ReturnsValidationFailureWithoutThrowing()
    {
        // GetScopedService (not a plain CreateScope) is required here: it populates a fake
        // authenticated HttpContext with the fixed TestIds tenant so TenantIsolationBehavior
        // resolves a tenant context to compare the command's TenantId against.
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        using var _ = scope;

        var command = RecordCognitiveLoadReadingCommand.FromDto(
            tenantId: TenantId, siteId: null, shiftType: "NotAShift", shiftDateUtc: DateTime.UtcNow,
            workerUserId: Guid.NewGuid(), taskComplexityProfileId: null, cognitiveLoadScore: 50m,
            measurementMethod: "SelfReport", measuredAtUtc: DateTime.UtcNow, requesterUserId: ActorId);

        var result = await sender.Send(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.InvalidShiftType");
        result.Error.Severity.Should().Be(ErrorSeverity.Validation);
    }

    [Fact]
    public async Task AddAsync_ReSubmittingIdenticalChildCollectionTwice_DoesNotDuplicateReadingRows()
    {
        // Failover/Recovery: CognitiveLoadLedgerDapperRepository's precedent is to re-submit the
        // aggregate's full in-memory child collection on every Add/Update call, relying on each
        // child Insert proc's IF NOT EXISTS guard to make retries safe. A caller that retries a
        // failed UpdateAsync (e.g. after a transient connection drop) must not end up with
        // duplicated reading rows.
        using var scope = Factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICognitiveLoadLedgerRepository>();

        var ledger = CognitiveLoadLedger.Create(
            UserId.FromGuid(ActorId), UtcTimestamp.Now(), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()),
            PerformedByActor.User(UserId.FromGuid(ActorId)), TenantId, null, ShiftType.Night, UtcTimestamp.Now());
        ledger.RecordCognitiveLoadReading(
            UserId.FromGuid(Guid.NewGuid()), null, 45m, "SelfReport", UtcTimestamp.Now(), UtcTimestamp.Now(),
            UserId.FromGuid(ActorId), PerformedByActor.User(UserId.FromGuid(ActorId)), new CorrelationId(CorrelationId), new CausationId(Guid.NewGuid()));
        await repo.AddAsync(ledger, CancellationToken.None);

        // Simulate a retried UpdateAsync call with the identical in-memory aggregate (same
        // reading id already present in _cognitiveLoadReadings).
        await repo.UpdateAsync(ledger, CancellationToken.None);
        await repo.UpdateAsync(ledger, CancellationToken.None);

        var reloaded = await repo.GetByIdAsync(ledger.CognitiveLoadLedgerId, TenantId, CancellationToken.None);

        reloaded.Should().NotBeNull();
        reloaded!.CognitiveLoadReadings.Should().ContainSingle("repeated UpdateAsync retries must not duplicate the already-persisted reading row");
    }

    [Fact]
    public async Task RecalculateWorkloadTrendCommandHandler_ForUnknownLedgerId_ReturnsNotFoundWithoutThrowing()
    {
        // Chaos: a caller referencing a ledger id that was never persisted (e.g. a stale client
        // cache or a replayed command against a torn-down tenant) must fail gracefully.
        // GetScopedService (not a plain CreateScope) is required here: it populates a fake
        // authenticated HttpContext with the fixed TestIds tenant so TenantIsolationBehavior
        // resolves a tenant context to compare the command's TenantId against.
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        using var _ = scope;

        var command = RecalculateWorkloadTrendCommand.FromDto(
            tenantId: TenantId, cognitiveLoadLedgerId: Guid.NewGuid(),
            periodStartUtc: DateTime.UtcNow.AddHours(-1), periodEndUtc: DateTime.UtcNow, requesterUserId: ActorId);

        var result = await sender.Send(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.CognitiveLoadLedger.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }
}
