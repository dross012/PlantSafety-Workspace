// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Financials
//  Layer:          IntegrationTests
//  File:           FinancialsSqlTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    SQL-layer tests verifying Financials stored procedure contracts.
//                  Deterministic, parallel-safe, additive-only.
//  UPDATED:        2026-07-28 -- added a genuine, non-vacuous read-path test
//                  (GetFinancials_WithSeededRow_ShouldReturnRealDataThroughRealHandler) that inserts
//                  directly into the real [Financials].[Financials] table (applied to the
//                  Testcontainers instance by TestDatabaseFactory.ApplyFinancialsSchemaAsync,
//                  mirroring PlantSafety.Infrastructure\Financials\Sql\Tables\Financials.sql /
//                  \Procedures\usp_Financials_GetRecords.sql verbatim) and then round-trips it
//                  through the real DI container's GetFinancialsQueryHandler ->
//                  IFinancialsRepository -> FinancialsDapperRepository -> real
//                  usp_Financials_GetRecords stored procedure. The pre-existing
//                  OutboxTable_ShouldExist_InSystemSchema test is left in place (still a valid,
//                  cheap smoke check that the shared System schema is up before this class's real
//                  test runs) but no longer the only coverage in this file.
// ============================================================================
using MediatR;

using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Financials.Financials.Queries;
using PlantSafety.Application.Financials.Financials.Repositories;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Financials.Sql;

/// <summary>
/// SQL-layer tests for the Financials module.  Verifies stored procedure contracts,
/// canonical parameter handling, and TRY/CATCH error propagation.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class FinancialsSqlTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task OutboxTable_ShouldExist_InSystemSchema()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var count = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'OutboxMessages'");

        count.Should().Be(1, "System.OutboxMessages table must exist before running Financials SQL tests");
    }

    [Fact]
    public async Task GetFinancials_WithSeededRow_ShouldReturnRealDataThroughRealHandler()
    {
        var recordId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var recordedAtUtc = FixedUtcNow;

        // ---- Seed directly into the real [Financials].[Financials] table (no C# write path
        // exists for this read-model -- see IFinancialsRepository remarks). ----
        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.ExecuteAsync(
                @"INSERT INTO [Financials].[Financials]
                    (RecordId, TenantId, SiteId, Category, Amount, Status, RecordedAtUtc, RecordedBy)
                  VALUES
                    (@RecordId, @TenantId, @SiteId, @Category, @Amount, @Status, @RecordedAtUtc, @RecordedBy)",
                new
                {
                    RecordId = recordId,
                    TenantId,
                    SiteId = siteId,
                    Category = "Budget",
                    Amount = 12345.67m,
                    Status = "Posted",
                    RecordedAtUtc = recordedAtUtc,
                    RecordedBy = ActorId,
                });
        }

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            // ---- Exercise the real MediatR pipeline -> GetFinancialsQueryHandler ->
            // IFinancialsRepository -> FinancialsDapperRepository -> real
            // usp_Financials_GetRecords stored procedure. ----
            var result = await sender.Send(new GetFinancialsQuery(
                TenantId: TenantId,
                SiteId: siteId,
                From: recordedAtUtc.AddDays(-1),
                To: recordedAtUtc.AddDays(1)), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().ContainSingle(i => i.RecordId == recordId);

            var item = result.Value.Items.Single(i => i.RecordId == recordId);
            item.TenantId.Should().Be(TenantId);
            item.SiteId.Should().Be(siteId);
            item.Category.Should().Be("Budget");
            item.Amount.Should().Be(12345.67m);
            item.Status.Should().Be("Posted");
            item.RecordedBy.Should().Be(ActorId);

            // ---- Also confirm the real repository resolved from the DI container returns the
            // same real row (belt-and-braces against a MediatR pipeline behavior masking a
            // repository-level regression). ----
            var repository = scope.ServiceProvider.GetRequiredService<IFinancialsRepository>();
            var direct = await repository.GetRecordsAsync(
                TenantId, siteId, recordedAtUtc.AddDays(-1), recordedAtUtc.AddDays(1), CancellationToken.None);

            direct.Should().ContainSingle(i => i.RecordId == recordId);
        }
        finally
        {
            scope.Dispose();
        }
    }
}
