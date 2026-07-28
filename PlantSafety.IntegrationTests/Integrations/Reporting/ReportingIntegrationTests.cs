// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Integrations.Reporting
//  Layer:          IntegrationTests
//  File:           ReportingIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  UPDATED:        2026-07-28 (Claude Code migration-commander-agent) -- REPLACES the prior
//                  vacuous generic outbox-insert stub with real Phase 3 SQL Modernization
//                  coverage for the live, read-only Integrations.Reporting path
//                  (IReportingRepository -> ReportingDapperRepository ->
//                  [Integrations].[ReportingItems] / usp_Reporting_GetReportDeliveries), consumed
//                  by GetReportingQueryHandler. This path has no write procedure, so
//                  ROWVERSION/ETag modernization does not apply here.
//  SchemaVersion:  2026.1
//  Description:    Exercises the real MediatR handler -> real repository -> real DI -> real
//                  database end to end, and forces a genuine SQL-level failure to prove the
//                  BEGIN TRY/BEGIN CATCH + ErrorLog + THROW modernization is wired up.
// ============================================================================
using MediatR;

using PlantSafety.Application.Integrations.Reporting.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Integrations.Reporting;

/// <summary>Integration tests for the <c>Reporting</c> module in the <c>Integrations</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class ReportingIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetReporting_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
    {
        var siteId = Guid.NewGuid();
        var initiatedBy = Guid.NewGuid();
        var inRangeId = Guid.NewGuid();
        var deletedId = Guid.NewGuid();
        var outOfRangeId = Guid.NewGuid();

        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.OpenAsync();

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[ReportingItems]
                    (DeliveryId, TenantId, SiteId, ReportName, RecipientSystem, DeliveryChannel, Status, DeliveredAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@DeliveryId, @TenantId, @SiteId, @ReportName, @RecipientSystem, @DeliveryChannel, @Status, @DeliveredAtUtc, @InitiatedBy, 0)",
                new
                {
                    DeliveryId = inRangeId,
                    TenantId,
                    SiteId = siteId,
                    ReportName = "Real Seeded Monthly Safety Report",
                    RecipientSystem = "RegulatoryPortal",
                    DeliveryChannel = "SFTP",
                    Status = "Sent",
                    DeliveredAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[ReportingItems]
                    (DeliveryId, TenantId, SiteId, ReportName, RecipientSystem, DeliveryChannel, Status, DeliveredAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@DeliveryId, @TenantId, @SiteId, @ReportName, @RecipientSystem, @DeliveryChannel, @Status, @DeliveredAtUtc, @InitiatedBy, 1)",
                new
                {
                    DeliveryId = deletedId,
                    TenantId,
                    SiteId = siteId,
                    ReportName = "Deleted Report",
                    RecipientSystem = "RegulatoryPortal",
                    DeliveryChannel = "Email",
                    Status = "Sent",
                    DeliveredAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[ReportingItems]
                    (DeliveryId, TenantId, SiteId, ReportName, RecipientSystem, DeliveryChannel, Status, DeliveredAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@DeliveryId, @TenantId, @SiteId, @ReportName, @RecipientSystem, @DeliveryChannel, @Status, @DeliveredAtUtc, @InitiatedBy, 0)",
                new
                {
                    DeliveryId = outOfRangeId,
                    TenantId,
                    SiteId = siteId,
                    ReportName = "Out Of Range Report",
                    RecipientSystem = "API",
                    DeliveryChannel = "API",
                    Status = "Failed",
                    DeliveredAtUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });
        }

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new GetReportingQuery(TenantId, siteId, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetReportingQueryHandler must succeed against the real stored procedure");
            result.Value!.Items.Should().Contain(i => i.DeliveryId == inRangeId,
                "the in-range, non-deleted seeded row must be returned by the real usp_Reporting_GetReportDeliveries");
            result.Value.Items.Should().NotContain(i => i.DeliveryId == deletedId,
                "usp_Reporting_GetReportDeliveries filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.DeliveryId == outOfRangeId,
                "usp_Reporting_GetReportDeliveries filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.DeliveryId == inRangeId);
            item.ReportName.Should().Be("Real Seeded Monthly Safety Report");
            item.RecipientSystem.Should().Be("RegulatoryPortal");
            item.DeliveryChannel.Should().Be("SFTP");
            item.Status.Should().Be("Sent");
            item.InitiatedBy.Should().Be(initiatedBy);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetReporting_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var noDataTenantId = Guid.NewGuid();
        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new GetReportingQuery(noDataTenantId, null, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded ReportingItems rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetReportDeliveries_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Reporting_GetReportDeliveries]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        DeliveryId,
                        TenantId,
                        SiteId,
                        ReportName,
                        RecipientSystem,
                        DeliveryChannel,
                        Status,
                        DeliveredAtUtc,
                        InitiatedBy
                    FROM [Integrations].[ReportingItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND DeliveredAtUtc >= @FromUtc
                      AND DeliveredAtUtc <= @ToUtc
                    ORDER BY DeliveredAtUtc DESC;
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
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Reporting_GetReportDeliveries]
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
                        DeliveryId,
                        TenantId,
                        SiteId,
                        ReportName,
                        RecipientSystem,
                        DeliveryChannel,
                        Status,
                        DeliveredAtUtc,
                        InitiatedBy
                    FROM [Integrations].[ReportingItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND DeliveredAtUtc >= @FromUtc
                      AND DeliveredAtUtc <= @ToUtc
                    ORDER BY DeliveredAtUtc DESC;
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
                await using var cmd = new SqlCommand("[Integrations].[usp_Reporting_GetReportDeliveries]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_Reporting_GetReportDeliveries must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Reporting_GetReportDeliveries", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Reporting_GetReportDeliveries's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}
