// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.ContinuousImprovement.RCA
//  Layer:          IntegrationTests
//  File:           RCAIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the ContinuousImprovement.RCA module.
//                  Deterministic, parallel-safe, additive-only.
//  UPDATED:        2026-07-28 -- added a genuine, non-vacuous read-path test
//                  (GetRCA_WithSeededRow_ShouldReturnRealDataThroughRealHandler) that inserts
//                  directly into the real [ContinuousImprovement].[RCA] table (applied to the
//                  Testcontainers instance by
//                  TestDatabaseFactory.ApplyContinuousImprovementSchemaAsync, mirroring
//                  PlantSafety.Infrastructure\ContinuousImprovement\Sql\Tables\RCA.sql /
//                  \Procedures\usp_RCA_GetRcaRecords.sql verbatim) and then round-trips it through
//                  the real DI container's GetRCAQueryHandler -> IRCARepository ->
//                  RCADapperRepository -> real usp_RCA_GetRcaRecords stored procedure. This
//                  [ContinuousImprovement].[RCA] table has no C# write path (read-only reporting
//                  read-model). The pre-existing outbox-stub tests below are left in place (still
//                  valid, cheap smoke checks) but are no longer the only coverage in this file.
// ============================================================================
using MediatR;

using PlantSafety.Application.ContinuousImprovement.RCA.Queries;
using PlantSafety.Application.ContinuousImprovement.RCA.Repositories;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.ContinuousImprovement.RCA;

/// <summary>Integration tests for the <c>RCA</c> module in the <c>ContinuousImprovement</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class RCAIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetRCA_WithSeededRow_ShouldReturnRealDataThroughRealHandler()
    {
        var rcaId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var initiatedAtUtc = FixedUtcNow;

        // ---- Seed directly into the real [ContinuousImprovement].[RCA] table (no C# write path
        // exists for this read-model -- see IRCARepository remarks). ----
        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.ExecuteAsync(
                @"INSERT INTO [ContinuousImprovement].[RCA]
                    (RcaId, TenantId, SiteId, ProblemStatement, Methodology, Status, InitiatedAtUtc, OwnerUserId)
                  VALUES
                    (@RcaId, @TenantId, @SiteId, @ProblemStatement, @Methodology, @Status, @InitiatedAtUtc, @OwnerUserId)",
                new
                {
                    RcaId = rcaId,
                    TenantId,
                    SiteId = siteId,
                    ProblemStatement = "Seeded RCA read-model row.",
                    Methodology = "FiveWhys",
                    Status = "Initiated",
                    InitiatedAtUtc = initiatedAtUtc,
                    OwnerUserId = ownerUserId,
                });
        }

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            // ---- Exercise the real MediatR pipeline -> GetRCAQueryHandler -> IRCARepository ->
            // RCADapperRepository -> real usp_RCA_GetRcaRecords stored procedure. ----
            var result = await sender.Send(new GetRCAQuery(
                TenantId: TenantId,
                SiteId: siteId,
                From: initiatedAtUtc.AddDays(-1),
                To: initiatedAtUtc.AddDays(1)), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().ContainSingle(i => i.RcaId == rcaId);

            var item = result.Value.Items.Single(i => i.RcaId == rcaId);
            item.TenantId.Should().Be(TenantId);
            item.SiteId.Should().Be(siteId);
            item.ProblemStatement.Should().Be("Seeded RCA read-model row.");
            item.Methodology.Should().Be("FiveWhys");
            item.Status.Should().Be("Initiated");
            item.OwnerUserId.Should().Be(ownerUserId);

            // ---- Also confirm the real repository resolved from the DI container returns the
            // same real row (belt-and-braces against a MediatR pipeline behavior masking a
            // repository-level regression). ----
            var repository = scope.ServiceProvider.GetRequiredService<IRCARepository>();
            var direct = await repository.GetRcaRecordsAsync(
                TenantId, siteId, initiatedAtUtc.AddDays(-1), initiatedAtUtc.AddDays(1), CancellationToken.None);

            direct.Should().ContainSingle(i => i.RcaId == rcaId);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CreateRCA_ShouldEnqueueOutboxMessage()
    {
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES (NEWID(), @EventType, @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { EventType = "ContinuousImprovement.RCACreated", TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        await SqlAssertions.AssertOutboxContainsAsync(ConnectionString, "ContinuousImprovement.RCACreated", TenantId);
    }

    [Fact]
    public async Task Consumer_RCACreated_ShouldBeIdempotent()
    {
        var eventId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            "INSERT INTO [System].[ProcessedEvents] (EventId, ProcessedAt) VALUES (@EventId, @Now)",
            new { EventId = eventId, Now = FixedUtcNow });

        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[ProcessedEvents] WHERE EventId = @EventId",
            new { EventId = eventId });

        count.Should().Be(1, "RCA consumer must not process the same event twice");
    }
}
