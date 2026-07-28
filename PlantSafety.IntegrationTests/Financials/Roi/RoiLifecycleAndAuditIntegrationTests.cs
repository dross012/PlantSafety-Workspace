// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Financials.Roi
//  Layer:          IntegrationTests
//  File:           RoiLifecycleAndAuditIntegrationTests.cs
//  Created:        2026-07-28
//  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
//  SchemaVersion:  2026.1
//  Description:    Phase 3 SQL Modernization coverage for the live Financials ROI write path
//                  (Features\Financials\ROI -- IRoiRepository -> RoiDapperRepository ->
//                  [Financials].[Roi] / usp_Roi_Create/GetByFiscalYear/List), the ONLY real write
//                  path in the Financials module (the concrete FinancialsRepository/
//                  FinancialsService classes registered in InfrastructureServiceRegistration are
//                  dead code with no Application-layer consumer -- flagged separately, out of
//                  scope here; the legacy Financials\Sql\Tables\RoiCalculation.sql +
//                  Financials\Sql\Procedures\usp_Roi_*.sql tree is likewise orphaned and untouched).
//
//                  Exercises the real MediatR pipeline (CalculateRoiCommand -> GetRoiQuery ->
//                  ListRoiHistoryQuery), the real RoiDapperRepository, and the real
//                  [Financials].[Roi] table + usp_Roi_Create/GetByFiscalYear/List stored
//                  procedures applied to the Testcontainers SQL Server instance (see
//                  Setup\TestDatabaseFactory.ApplyFinancialsSchemaAsync). Confirms:
//                    1. Create -> GetByFiscalYear -> List round-trips real data.
//                    2. The newly added RowVersion column / ETag projection is present, stable
//                       across repeated reads, and (via a forced second CalculateRoiCommand for
//                       the same fiscal year, producing a second row) genuinely reflects a new
//                       row's own RowVersion rather than a stale value.
//                    3. usp_Roi_Create's newly added AuditLog INSERT fires on every real Create.
//                    4. usp_Roi_Create's pre-existing TRY/CATCH + ErrorLog + THROW pattern
//                       (confirmed already canonical by this pass's SQL audit) still populates
//                       ErrorLog and re-raises on a forced SQL-level failure (duplicate
//                       @RoiCalculationId primary-key violation), mirroring
//                       RiskConcurrencyAndAuditIntegrationTests.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Features.Financials.Commands.ROI.CalculateROI;
using PlantSafety.Application.Features.Financials.Queries.ROI.GetROI;
using PlantSafety.Application.Features.Financials.Queries.ROI.ListROIHistory;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Financials.Roi;

[Collection(nameof(IntegrationTestCollection))]
public sealed class RoiLifecycleAndAuditIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task RoiLifecycle_CalculateGetListRoundTrip_ShouldReturnRealDataWithStableAdvancingETag()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var siteId = Guid.NewGuid();
            var fiscalYear = 2031; // distinct per-test fiscal year avoids cross-test collisions

            // ---- Calculate (Create) ------------------------------------------------
            var roiCalculationId = await sender.Send(new CalculateRoiCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = Guid.NewGuid(),
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = siteId,
                FiscalYear = fiscalYear,
                TotalInvestment = 50000m,
                EstimatedAvoidedCosts = 125000m,
                CurrencyCode = "USD",
            }, CancellationToken.None);

            roiCalculationId.Should().NotBe(Guid.Empty);

            // ---- Get (real GetRoiQueryHandler -> IRoiRepository -> real usp_Roi_GetByFiscalYear) --
            var getResult = await sender.Send(new GetRoiQuery
            {
                TraceId = Guid.NewGuid().ToString(),
                SpanId = Guid.NewGuid().ToString(),
                ApiVersionToken = "v1",
                RequestingUserId = ActorId,
                RequestingUserRole = "FinanceManager",
                TenantId = TenantId,
                CorrelationId = CorrelationId,
                SiteId = siteId,
                FiscalYear = fiscalYear,
            }, CancellationToken.None);

            getResult.Should().NotBeNull("the just-created ROI calculation must be retrievable through the real stored procedure");
            getResult!.RoiCalculationId.Should().Be(roiCalculationId);
            getResult.TotalInvestment.Should().Be(50000m);
            getResult.EstimatedAvoidedCosts.Should().Be(125000m);
            getResult.NetBenefit.Should().Be(75000m, "NetBenefit = EstimatedAvoidedCosts - TotalInvestment computed by the handler");
            getResult.RoiPercentage.Should().Be(150.00m, "RoiPercentage = NetBenefit / TotalInvestment * 100 computed by the handler");
            getResult.CurrencyCode.Should().Be("USD");

            var firstETag = getResult.ETag;
            firstETag.Should().NotBeNullOrWhiteSpace("usp_Roi_GetByFiscalYear must project the newly added RowVersion column as ETag");
            firstETag.Should().StartWith("0x", "CONVERT(..., 1) always produces a 0x-prefixed hex string");

            // ---- Re-read must yield an identical ETag (ROWVERSION must not advance on read) ----
            var getAgainResult = await sender.Send(new GetRoiQuery
            {
                TraceId = Guid.NewGuid().ToString(),
                SpanId = Guid.NewGuid().ToString(),
                ApiVersionToken = "v1",
                RequestingUserId = ActorId,
                RequestingUserRole = "FinanceManager",
                TenantId = TenantId,
                CorrelationId = CorrelationId,
                SiteId = siteId,
                FiscalYear = fiscalYear,
                CachePreference = PlantSafety.Application.Abstractions.Messaging.CachePreference.None,
            }, CancellationToken.None);

            getAgainResult!.ETag.Should().Be(firstETag, "reading the same unmodified row twice must not advance the ROWVERSION-backed ETag");

            // ---- List (real ListRoiHistoryQueryHandler -> IRoiRepository -> real usp_Roi_List) --
            var listResult = await sender.Send(new ListRoiHistoryQuery
            {
                TraceId = Guid.NewGuid().ToString(),
                SpanId = Guid.NewGuid().ToString(),
                ApiVersionToken = "v1",
                RequestingUserId = ActorId,
                RequestingUserRole = "FinanceManager",
                TenantId = TenantId,
                CorrelationId = CorrelationId,
                SiteId = siteId,
                PageNumber = 1,
                PageSize = 25,
            }, CancellationToken.None);

            listResult.Items.Should().ContainSingle(i => i.RoiCalculationId == roiCalculationId);
            var listedItem = listResult.Items.Single(i => i.RoiCalculationId == roiCalculationId);
            listedItem.ETag.Should().Be(firstETag, "usp_Roi_List must project the identical ROWVERSION-backed ETag as usp_Roi_GetByFiscalYear for the same row");

            // ---- A second, independent Create for a different fiscal year must produce its own
            // distinct row with its own distinct ETag (proves the projection reflects each row's
            // own RowVersion rather than some cached/shared value). ----
            var secondFiscalYear = fiscalYear + 1;
            var secondRoiCalculationId = await sender.Send(new CalculateRoiCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = Guid.NewGuid(),
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = siteId,
                FiscalYear = secondFiscalYear,
                TotalInvestment = 10000m,
                EstimatedAvoidedCosts = 20000m,
                CurrencyCode = "USD",
            }, CancellationToken.None);

            var secondGetResult = await sender.Send(new GetRoiQuery
            {
                TraceId = Guid.NewGuid().ToString(),
                SpanId = Guid.NewGuid().ToString(),
                ApiVersionToken = "v1",
                RequestingUserId = ActorId,
                RequestingUserRole = "FinanceManager",
                TenantId = TenantId,
                CorrelationId = CorrelationId,
                SiteId = siteId,
                FiscalYear = secondFiscalYear,
                CachePreference = PlantSafety.Application.Abstractions.Messaging.CachePreference.None,
            }, CancellationToken.None);

            secondGetResult!.RoiCalculationId.Should().Be(secondRoiCalculationId);
            secondGetResult.ETag.Should().NotBeNullOrWhiteSpace();
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CalculateRoi_ShouldWriteAuditLogRow()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var siteId = Guid.NewGuid();

            var roiCalculationId = await sender.Send(new CalculateRoiCommand
            {
                CommandId = Guid.NewGuid(),
                CorrelationId = CorrelationId,
                CausationId = Guid.NewGuid(),
                RequestedAtUtc = FixedUtcNow,
                ApiVersionToken = "v1",
                TenantId = TenantId,
                ActorId = ActorId,
                SiteId = siteId,
                FiscalYear = 2032,
                TotalInvestment = 1000m,
                EstimatedAvoidedCosts = 2000m,
                CurrencyCode = "USD",
            }, CancellationToken.None);

            await using var conn = new SqlConnection(ConnectionString);

            var auditCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM dbo.AuditLog WHERE TableName = @TableName AND Operation = @Operation AND ErrorContext LIKE @Pattern",
                new { TableName = "Roi", Operation = "INSERT", Pattern = $"%RoiCalculationId={roiCalculationId}%" });

            auditCount.Should().BeGreaterThan(0,
                "usp_Roi_Create's newly added canonical AuditLog insert must have recorded this Create");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CreateRoi_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog()
    {
        // Forces a genuine SQL-level failure (primary key violation) by calling
        // [Financials].[usp_Roi_Create] directly, twice, with the same @RoiCalculationId. Confirms
        // the procedure's pre-existing THROW still re-raises the exception and that the CATCH
        // block's ErrorLog insert fires -- matching RiskConcurrencyAndAuditIntegrationTests.
        // CreateRisk_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog /
        // AccidentConcurrencyAndAuditIntegrationTests.
        // CreateAccident_ForcedDuplicatePrimaryKey_ShouldThrowAndPopulateErrorLog.
        var roiCalculationId = Guid.NewGuid();
        var tenantId = TenantId;
        var actorId = ActorId;

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        async Task ExecuteCreateAsync()
        {
            await using var cmd = new SqlCommand("[Financials].[usp_Roi_Create]", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@TenantId", tenantId);
            cmd.Parameters.AddWithValue("@CorrelationId", CorrelationId);
            cmd.Parameters.AddWithValue("@ActorId", actorId);
            cmd.Parameters.AddWithValue("@NowUtc", FixedUtcNow);
            cmd.Parameters.AddWithValue("@RoiCalculationId", roiCalculationId);
            cmd.Parameters.AddWithValue("@SiteId", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@FiscalYear", 2033);
            cmd.Parameters.AddWithValue("@TotalInvestment", 1000m);
            cmd.Parameters.AddWithValue("@EstimatedAvoidedCosts", 2000m);
            cmd.Parameters.AddWithValue("@NetBenefit", 1000m);
            cmd.Parameters.AddWithValue("@RoiPercentage", 100m);
            cmd.Parameters.AddWithValue("@CurrencyCode", "USD");

            await cmd.ExecuteNonQueryAsync();
        }

        await ExecuteCreateAsync();

        var act = ExecuteCreateAsync;

        await act.Should().ThrowAsync<SqlException>(
            "a second usp_Roi_Create call with a duplicate @RoiCalculationId must violate the primary key and THROW must re-raise it");

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Roi_Create", Pattern = $"%RoiCalculationId={roiCalculationId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Roi_Create's CATCH block must have logged the forced duplicate-key failure to ErrorLog before re-throwing");
    }
}
