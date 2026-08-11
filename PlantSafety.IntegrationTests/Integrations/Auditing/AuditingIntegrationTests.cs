// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Integrations.Auditing
//  Layer:          IntegrationTests
//  File:           AuditingIntegrationTests.cs
//  Created:        2026-07-28
//  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
//  SchemaVersion:  2026.1
//  Description:    Phase 3 SQL Modernization coverage for the live, read-only Integrations.Auditing
//                  path (IAuditingRepository -> AuditingDapperRepository ->
//                  [Integrations].[AuditingItems] / usp_Auditing_GetLogs), consumed by
//                  GetAuditingQueryHandler. This path has no write procedure, so ROWVERSION/ETag
//                  modernization does not apply here -- matches the Intelligence read-only
//                  precedent. Exercises the real MediatR handler -> real repository -> real DI ->
//                  real database end to end, and forces a genuine SQL-level failure to prove the
//                  BEGIN TRY/BEGIN CATCH + ErrorLog + THROW modernization is wired up.
// ============================================================================
using MediatR;

using PlantSafety.Application.Integrations.Auditing.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Integrations.Auditing;

/// <summary>Integration tests for the <c>Auditing</c> module in the <c>Integrations</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class AuditingIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAuditing_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
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
                @"INSERT INTO [Integrations].[AuditingItems]
                    (LogId, TenantId, SiteId, ExternalSystem, Direction, Endpoint, Status, OccurredAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@LogId, @TenantId, @SiteId, @ExternalSystem, @Direction, @Endpoint, @Status, @OccurredAtUtc, @InitiatedBy, 0)",
                new
                {
                    LogId = inRangeId,
                    TenantId,
                    SiteId = siteId,
                    ExternalSystem = "Real Seeded ExternalSystem",
                    Direction = "Inbound",
                    Endpoint = "/webhooks/receive",
                    Status = "Success",
                    OccurredAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[AuditingItems]
                    (LogId, TenantId, SiteId, ExternalSystem, Direction, Endpoint, Status, OccurredAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@LogId, @TenantId, @SiteId, @ExternalSystem, @Direction, @Endpoint, @Status, @OccurredAtUtc, @InitiatedBy, 1)",
                new
                {
                    LogId = deletedId,
                    TenantId,
                    SiteId = siteId,
                    ExternalSystem = "Deleted ExternalSystem",
                    Direction = "Outbound",
                    Endpoint = "/api/export",
                    Status = "Success",
                    OccurredAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[AuditingItems]
                    (LogId, TenantId, SiteId, ExternalSystem, Direction, Endpoint, Status, OccurredAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@LogId, @TenantId, @SiteId, @ExternalSystem, @Direction, @Endpoint, @Status, @OccurredAtUtc, @InitiatedBy, 0)",
                new
                {
                    LogId = outOfRangeId,
                    TenantId,
                    SiteId = siteId,
                    ExternalSystem = "Out Of Range ExternalSystem",
                    Direction = "Inbound",
                    Endpoint = "/webhooks/receive",
                    Status = "Failed",
                    OccurredAtUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });
        }

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetAuditingQuery(TenantId, siteId, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetAuditingQueryHandler must succeed against the real stored procedure");
            result.Value!.Items.Should().Contain(i => i.LogId == inRangeId,
                "the in-range, non-deleted seeded row must be returned by the real usp_Auditing_GetLogs");
            result.Value.Items.Should().NotContain(i => i.LogId == deletedId,
                "usp_Auditing_GetLogs filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.LogId == outOfRangeId,
                "usp_Auditing_GetLogs filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.LogId == inRangeId);
            item.ExternalSystem.Should().Be("Real Seeded ExternalSystem");
            item.Direction.Should().Be("Inbound");
            item.Endpoint.Should().Be("/webhooks/receive");
            item.Status.Should().Be("Success");
            item.InitiatedBy.Should().Be(initiatedBy);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetAuditing_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var fromUtc = new DateTime(2035, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2035, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetAuditingQuery(TenantId, null, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded AuditingItems rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetLogs_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Auditing_GetLogs]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        LogId,
                        TenantId,
                        SiteId,
                        ExternalSystem,
                        Direction,
                        Endpoint,
                        Status,
                        OccurredAtUtc,
                        InitiatedBy
                    FROM [Integrations].[AuditingItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND OccurredAtUtc >= @FromUtc
                      AND OccurredAtUtc <= @ToUtc
                    ORDER BY OccurredAtUtc DESC;
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
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Auditing_GetLogs]
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
                        LogId,
                        TenantId,
                        SiteId,
                        ExternalSystem,
                        Direction,
                        Endpoint,
                        Status,
                        OccurredAtUtc,
                        InitiatedBy
                    FROM [Integrations].[AuditingItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND OccurredAtUtc >= @FromUtc
                      AND OccurredAtUtc <= @ToUtc
                    ORDER BY OccurredAtUtc DESC;
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
                await using var cmd = new SqlCommand("[Integrations].[usp_Auditing_GetLogs]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_Auditing_GetLogs must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Auditing_GetLogs", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Auditing_GetLogs's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}
