// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Integrations.Analytics
//  Layer:          IntegrationTests
//  File:           AnalyticsIntegrationTests.cs
//  Created:        2026-07-28
//  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
//  SchemaVersion:  2026.1
//  Description:    Phase 3 SQL Modernization coverage for the live, read-only Integrations.Analytics
//                  path (IAnalyticsRepository -> AnalyticsDapperRepository ->
//                  [Integrations].[AnalyticsItems] / usp_Analytics_GetRecords), consumed by
//                  GetAnalyticsQueryHandler. This path has no write procedure, so ROWVERSION/ETag
//                  modernization does not apply here -- matches the Intelligence read-only
//                  precedent. Exercises the real MediatR handler -> real repository -> real DI ->
//                  real database end to end, and forces a genuine SQL-level failure to prove the
//                  BEGIN TRY/BEGIN CATCH + ErrorLog + THROW modernization is wired up.
// ============================================================================
using MediatR;

using PlantSafety.Application.Integrations.Analytics.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Integrations.Analytics;

/// <summary>Integration tests for the <c>Analytics</c> module in the <c>Integrations</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class AnalyticsIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAnalytics_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
    {
        var siteId = Guid.NewGuid();
        var ingestedBy = Guid.NewGuid();
        var inRangeId = Guid.NewGuid();
        var deletedId = Guid.NewGuid();
        var outOfRangeId = Guid.NewGuid();

        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.OpenAsync();

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[AnalyticsItems]
                    (RecordId, TenantId, SiteId, SourceSystem, MetricName, MetricValue, Status, IngestedAtUtc, IngestedBy, IsDeleted)
                  VALUES (@RecordId, @TenantId, @SiteId, @SourceSystem, @MetricName, @MetricValue, @Status, @IngestedAtUtc, @IngestedBy, 0)",
                new
                {
                    RecordId = inRangeId,
                    TenantId,
                    SiteId = siteId,
                    SourceSystem = "Real Seeded Analytics Source",
                    MetricName = "IncidentRate",
                    MetricValue = 3.25m,
                    Status = "Processed",
                    IngestedAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    IngestedBy = ingestedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[AnalyticsItems]
                    (RecordId, TenantId, SiteId, SourceSystem, MetricName, MetricValue, Status, IngestedAtUtc, IngestedBy, IsDeleted)
                  VALUES (@RecordId, @TenantId, @SiteId, @SourceSystem, @MetricName, @MetricValue, @Status, @IngestedAtUtc, @IngestedBy, 1)",
                new
                {
                    RecordId = deletedId,
                    TenantId,
                    SiteId = siteId,
                    SourceSystem = "Deleted Analytics Source",
                    MetricName = "IncidentRate",
                    MetricValue = 1.0m,
                    Status = "Processed",
                    IngestedAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    IngestedBy = ingestedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[AnalyticsItems]
                    (RecordId, TenantId, SiteId, SourceSystem, MetricName, MetricValue, Status, IngestedAtUtc, IngestedBy, IsDeleted)
                  VALUES (@RecordId, @TenantId, @SiteId, @SourceSystem, @MetricName, @MetricValue, @Status, @IngestedAtUtc, @IngestedBy, 0)",
                new
                {
                    RecordId = outOfRangeId,
                    TenantId,
                    SiteId = siteId,
                    SourceSystem = "Out Of Range Analytics Source",
                    MetricName = "IncidentRate",
                    MetricValue = 9.0m,
                    Status = "Pending",
                    IngestedAtUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    IngestedBy = ingestedBy,
                });
        }

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetAnalyticsQuery(TenantId, siteId, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetAnalyticsQueryHandler must succeed against the real stored procedure");
            result.Value!.Items.Should().Contain(i => i.RecordId == inRangeId,
                "the in-range, non-deleted seeded row must be returned by the real usp_Analytics_GetRecords");
            result.Value.Items.Should().NotContain(i => i.RecordId == deletedId,
                "usp_Analytics_GetRecords filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.RecordId == outOfRangeId,
                "usp_Analytics_GetRecords filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.RecordId == inRangeId);
            item.SourceSystem.Should().Be("Real Seeded Analytics Source");
            item.MetricName.Should().Be("IncidentRate");
            item.MetricValue.Should().Be(3.25m);
            item.Status.Should().Be("Processed");
            item.IngestedBy.Should().Be(ingestedBy);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetAnalytics_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var fromUtc = new DateTime(2035, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2035, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetAnalyticsQuery(TenantId, null, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded AnalyticsItems rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetRecords_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        // Forces a genuine, catchable SQL-level runtime failure (divide-by-zero) inside
        // usp_Analytics_GetRecords's TRY block by temporarily swapping in a procedure body that is
        // byte-for-byte identical except for one deliberately injected failing statement, then
        // calling the procedure directly and restoring the exact production body afterwards
        // (try/finally). See AITrainingIntegrationTests for why the naive "rename a referenced
        // column" approach does not work (SQL Server's TRY...CATCH does not catch statement-level
        // recompilation errors from deferred name resolution).
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Analytics_GetRecords]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        RecordId,
                        TenantId,
                        SiteId,
                        SourceSystem,
                        MetricName,
                        MetricValue,
                        Status,
                        IngestedAtUtc,
                        IngestedBy
                    FROM [Integrations].[AnalyticsItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND IngestedAtUtc >= @FromUtc
                      AND IngestedAtUtc <= @ToUtc
                    ORDER BY IngestedAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END";

        const string forcedFailureProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Analytics_GetRecords]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    DECLARE @ForcedFailureDivisor INT = 0;
                    DECLARE @ForcedFailureResult INT = 1 / @ForcedFailureDivisor; -- deliberate forced runtime failure for test coverage

                    SELECT
                        RecordId,
                        TenantId,
                        SiteId,
                        SourceSystem,
                        MetricName,
                        MetricValue,
                        Status,
                        IngestedAtUtc,
                        IngestedBy
                    FROM [Integrations].[AnalyticsItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND IngestedAtUtc >= @FromUtc
                      AND IngestedAtUtc <= @ToUtc
                    ORDER BY IngestedAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END";

        await conn.ExecuteAsync(forcedFailureProcedure);

        try
        {
            Func<Task> act = async () =>
            {
                await using var cmd = new SqlCommand("[Integrations].[usp_Analytics_GetRecords]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_Analytics_GetRecords must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Analytics_GetRecords", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Analytics_GetRecords's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}
