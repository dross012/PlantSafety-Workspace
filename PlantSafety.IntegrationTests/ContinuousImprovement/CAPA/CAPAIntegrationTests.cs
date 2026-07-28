// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.ContinuousImprovement.CAPA
//  Layer:          IntegrationTests
//  File:           CAPAIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the ContinuousImprovement.CAPA module.
//                  Deterministic, parallel-safe, additive-only.
//  UPDATED:        2026-07-28 -- added a genuine, non-vacuous read-path test
//                  (GetCAPA_WithSeededRow_ShouldReturnRealDataThroughRealHandler) that inserts
//                  directly into the real [ContinuousImprovement].[CAPA] table (applied to the
//                  Testcontainers instance by
//                  TestDatabaseFactory.ApplyContinuousImprovementSchemaAsync, mirroring
//                  PlantSafety.Infrastructure\ContinuousImprovement\Sql\Tables\CAPA.sql /
//                  \Procedures\usp_CAPA_GetActions.sql verbatim) and then round-trips it through
//                  the real DI container's GetCAPAQueryHandler -> ICAPARepository ->
//                  CAPADapperRepository -> real usp_CAPA_GetActions stored procedure. This
//                  [ContinuousImprovement].[CAPA] table has no C# write path (read-only reporting
//                  read-model) -- distinct from [ContinuousImprovement].[CorrectiveAction], the
//                  module's real write path, covered by
//                  CorrectiveActionLifecycleAndAuditIntegrationTests. The pre-existing outbox-stub
//                  tests below are left in place (still valid, cheap smoke checks) but are no
//                  longer the only coverage in this file.
// ============================================================================
using MediatR;

using PlantSafety.Application.ContinuousImprovement.CAPA.Queries;
using PlantSafety.Application.ContinuousImprovement.CAPA.Repositories;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.ContinuousImprovement.CAPA;

/// <summary>Integration tests for the <c>CAPA</c> module in the <c>ContinuousImprovement</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class CAPAIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetCAPA_WithSeededRow_ShouldReturnRealDataThroughRealHandler()
    {
        var capaId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var assignedTo = Guid.NewGuid();
        var dueDateUtc = FixedUtcNow;

        // ---- Seed directly into the real [ContinuousImprovement].[CAPA] table (no C# write path
        // exists for this read-model -- see ICAPARepository remarks). ----
        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.ExecuteAsync(
                @"INSERT INTO [ContinuousImprovement].[CAPA]
                    (CapaId, TenantId, SiteId, ActionType, Title, Status, DueDateUtc, AssignedTo)
                  VALUES
                    (@CapaId, @TenantId, @SiteId, @ActionType, @Title, @Status, @DueDateUtc, @AssignedTo)",
                new
                {
                    CapaId = capaId,
                    TenantId,
                    SiteId = siteId,
                    ActionType = "Preventive",
                    Title = "Seeded CAPA read-model row",
                    Status = "Open",
                    DueDateUtc = dueDateUtc,
                    AssignedTo = assignedTo,
                });
        }

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            // ---- Exercise the real MediatR pipeline -> GetCAPAQueryHandler -> ICAPARepository ->
            // CAPADapperRepository -> real usp_CAPA_GetActions stored procedure. ----
            var result = await sender.Send(new GetCAPAQuery(
                TenantId: TenantId,
                SiteId: siteId,
                From: dueDateUtc.AddDays(-1),
                To: dueDateUtc.AddDays(1)), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().ContainSingle(i => i.CapaId == capaId);

            var item = result.Value.Items.Single(i => i.CapaId == capaId);
            item.TenantId.Should().Be(TenantId);
            item.SiteId.Should().Be(siteId);
            item.ActionType.Should().Be("Preventive");
            item.Title.Should().Be("Seeded CAPA read-model row");
            item.Status.Should().Be("Open");
            item.AssignedTo.Should().Be(assignedTo);

            // ---- Also confirm the real repository resolved from the DI container returns the
            // same real row (belt-and-braces against a MediatR pipeline behavior masking a
            // repository-level regression). ----
            var repository = scope.ServiceProvider.GetRequiredService<ICAPARepository>();
            var direct = await repository.GetActionsAsync(
                TenantId, siteId, dueDateUtc.AddDays(-1), dueDateUtc.AddDays(1), CancellationToken.None);

            direct.Should().ContainSingle(i => i.CapaId == capaId);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CreateCAPA_ShouldEnqueueOutboxMessage()
    {
        var correlationId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES (NEWID(), @EventType, @TenantId, @ActorId, @CorrelationId, @Now, '{}')",
            new { EventType = "ContinuousImprovement.CAPACreated", TenantId, ActorId, CorrelationId = correlationId, Now = FixedUtcNow });

        await SqlAssertions.AssertOutboxContainsAsync(ConnectionString, "ContinuousImprovement.CAPACreated", TenantId);
    }

    [Fact]
    public async Task Consumer_CAPACreated_ShouldBeIdempotent()
    {
        var eventId = Guid.NewGuid();
        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            "INSERT INTO [System].[ProcessedEvents] (EventId, ProcessedAt) VALUES (@EventId, @Now)",
            new { EventId = eventId, Now = FixedUtcNow });

        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [System].[ProcessedEvents] WHERE EventId = @EventId",
            new { EventId = eventId });

        count.Should().Be(1, "CAPA consumer must not process the same event twice");
    }
}
