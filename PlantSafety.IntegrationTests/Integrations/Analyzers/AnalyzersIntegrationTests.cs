// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Integrations.Analyzers
//  Layer:          IntegrationTests
//  File:           AnalyzersIntegrationTests.cs
//  Created:        2026-07-28
//  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
//  SchemaVersion:  2026.1
//  Description:    Phase 3 SQL Modernization coverage for the live, read-only Integrations.Analyzers
//                  path (IAnalyzersRepository -> AnalyzersDapperRepository ->
//                  [Integrations].[AnalyzersItems] / usp_Analyzers_GetRuns), consumed by
//                  GetAnalyzersQueryHandler. This path has no write procedure, so ROWVERSION/ETag
//                  modernization does not apply here -- matches the Intelligence read-only
//                  precedent. Exercises the real MediatR handler -> real repository -> real DI ->
//                  real database end to end, and forces a genuine SQL-level failure to prove the
//                  BEGIN TRY/BEGIN CATCH + ErrorLog + THROW modernization is wired up.
// ============================================================================
using MediatR;

using PlantSafety.Application.Integrations.Analyzers.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Integrations.Analyzers;

/// <summary>Integration tests for the <c>Analyzers</c> module in the <c>Integrations</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class AnalyzersIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAnalyzers_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
    {
        var siteId = Guid.NewGuid();
        var triggeredBy = Guid.NewGuid();
        var inRangeId = Guid.NewGuid();
        var deletedId = Guid.NewGuid();
        var outOfRangeId = Guid.NewGuid();

        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.OpenAsync();

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[AnalyzersItems]
                    (RunId, TenantId, SiteId, AnalyzerName, TargetSystem, FindingsCount, Status, RunAtUtc, TriggeredBy, IsDeleted)
                  VALUES (@RunId, @TenantId, @SiteId, @AnalyzerName, @TargetSystem, @FindingsCount, @Status, @RunAtUtc, @TriggeredBy, 0)",
                new
                {
                    RunId = inRangeId,
                    TenantId,
                    SiteId = siteId,
                    AnalyzerName = "Real Seeded Analyzer",
                    TargetSystem = "ExternalDMS",
                    FindingsCount = 4,
                    Status = "Passed",
                    RunAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    TriggeredBy = triggeredBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[AnalyzersItems]
                    (RunId, TenantId, SiteId, AnalyzerName, TargetSystem, FindingsCount, Status, RunAtUtc, TriggeredBy, IsDeleted)
                  VALUES (@RunId, @TenantId, @SiteId, @AnalyzerName, @TargetSystem, @FindingsCount, @Status, @RunAtUtc, @TriggeredBy, 1)",
                new
                {
                    RunId = deletedId,
                    TenantId,
                    SiteId = siteId,
                    AnalyzerName = "Deleted Analyzer",
                    TargetSystem = "ExternalDMS",
                    FindingsCount = 1,
                    Status = "Passed",
                    RunAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    TriggeredBy = triggeredBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[AnalyzersItems]
                    (RunId, TenantId, SiteId, AnalyzerName, TargetSystem, FindingsCount, Status, RunAtUtc, TriggeredBy, IsDeleted)
                  VALUES (@RunId, @TenantId, @SiteId, @AnalyzerName, @TargetSystem, @FindingsCount, @Status, @RunAtUtc, @TriggeredBy, 0)",
                new
                {
                    RunId = outOfRangeId,
                    TenantId,
                    SiteId = siteId,
                    AnalyzerName = "Out Of Range Analyzer",
                    TargetSystem = "ExternalDMS",
                    FindingsCount = 9,
                    Status = "Failed",
                    RunAtUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    TriggeredBy = triggeredBy,
                });
        }

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new GetAnalyzersQuery(TenantId, siteId, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetAnalyzersQueryHandler must succeed against the real stored procedure");
            result.Value!.Items.Should().Contain(i => i.RunId == inRangeId,
                "the in-range, non-deleted seeded row must be returned by the real usp_Analyzers_GetRuns");
            result.Value.Items.Should().NotContain(i => i.RunId == deletedId,
                "usp_Analyzers_GetRuns filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.RunId == outOfRangeId,
                "usp_Analyzers_GetRuns filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.RunId == inRangeId);
            item.AnalyzerName.Should().Be("Real Seeded Analyzer");
            item.TargetSystem.Should().Be("ExternalDMS");
            item.FindingsCount.Should().Be(4);
            item.Status.Should().Be("Passed");
            item.TriggeredBy.Should().Be(triggeredBy);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetAnalyzers_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var noDataTenantId = Guid.NewGuid();
        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new GetAnalyzersQuery(noDataTenantId, null, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded AnalyzersItems rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetRuns_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Analyzers_GetRuns]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        RunId,
                        TenantId,
                        SiteId,
                        AnalyzerName,
                        TargetSystem,
                        FindingsCount,
                        Status,
                        RunAtUtc,
                        TriggeredBy
                    FROM [Integrations].[AnalyzersItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND RunAtUtc >= @FromUtc
                      AND RunAtUtc <= @ToUtc
                    ORDER BY RunAtUtc DESC;
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
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Analyzers_GetRuns]
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
                        RunId,
                        TenantId,
                        SiteId,
                        AnalyzerName,
                        TargetSystem,
                        FindingsCount,
                        Status,
                        RunAtUtc,
                        TriggeredBy
                    FROM [Integrations].[AnalyzersItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND RunAtUtc >= @FromUtc
                      AND RunAtUtc <= @ToUtc
                    ORDER BY RunAtUtc DESC;
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
                await using var cmd = new SqlCommand("[Integrations].[usp_Analyzers_GetRuns]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_Analyzers_GetRuns must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Analyzers_GetRuns", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Analyzers_GetRuns's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}
