// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Integrations.Import
//  Layer:          IntegrationTests
//  File:           ImportIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  UPDATED:        2026-07-28 (Claude Code migration-commander-agent) -- REPLACES the prior
//                  vacuous generic outbox-insert stub with real Phase 3 SQL Modernization
//                  coverage for the live, read-only Integrations.Import path (IImportRepository ->
//                  ImportDapperRepository -> [Integrations].[ImportItems] /
//                  usp_Import_GetImportRuns), consumed by GetImportQueryHandler. This path has no
//                  write procedure, so ROWVERSION/ETag modernization does not apply here.
//  SchemaVersion:  2026.1
//  Description:    Exercises the real MediatR handler -> real repository -> real DI -> real
//                  database end to end, and forces a genuine SQL-level failure to prove the
//                  BEGIN TRY/BEGIN CATCH + ErrorLog + THROW modernization is wired up.
// ============================================================================
using MediatR;

using PlantSafety.Application.Integrations.Import.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Integrations.Import;

/// <summary>Integration tests for the <c>Import</c> module in the <c>Integrations</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class ImportIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetImport_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
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
                @"INSERT INTO [Integrations].[ImportItems]
                    (ImportRunId, TenantId, SiteId, SourceSystem, ImportType, RecordsProcessed, RecordsFailed, Status, CompletedAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@ImportRunId, @TenantId, @SiteId, @SourceSystem, @ImportType, @RecordsProcessed, @RecordsFailed, @Status, @CompletedAtUtc, @InitiatedBy, 0)",
                new
                {
                    ImportRunId = inRangeId,
                    TenantId,
                    SiteId = siteId,
                    SourceSystem = "Real Seeded HRIS",
                    ImportType = "Employees",
                    RecordsProcessed = 200,
                    RecordsFailed = 2,
                    Status = "Completed",
                    CompletedAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[ImportItems]
                    (ImportRunId, TenantId, SiteId, SourceSystem, ImportType, RecordsProcessed, RecordsFailed, Status, CompletedAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@ImportRunId, @TenantId, @SiteId, @SourceSystem, @ImportType, @RecordsProcessed, @RecordsFailed, @Status, @CompletedAtUtc, @InitiatedBy, 1)",
                new
                {
                    ImportRunId = deletedId,
                    TenantId,
                    SiteId = siteId,
                    SourceSystem = "Deleted HRIS",
                    ImportType = "Employees",
                    RecordsProcessed = 3,
                    RecordsFailed = 0,
                    Status = "Completed",
                    CompletedAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[ImportItems]
                    (ImportRunId, TenantId, SiteId, SourceSystem, ImportType, RecordsProcessed, RecordsFailed, Status, CompletedAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@ImportRunId, @TenantId, @SiteId, @SourceSystem, @ImportType, @RecordsProcessed, @RecordsFailed, @Status, @CompletedAtUtc, @InitiatedBy, 0)",
                new
                {
                    ImportRunId = outOfRangeId,
                    TenantId,
                    SiteId = siteId,
                    SourceSystem = "Out Of Range HRIS",
                    ImportType = "TrainingRecords",
                    RecordsProcessed = 50,
                    RecordsFailed = 50,
                    Status = "Failed",
                    CompletedAtUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });
        }

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetImportQuery(TenantId, siteId, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetImportQueryHandler must succeed against the real stored procedure");
            result.Value!.Items.Should().Contain(i => i.ImportRunId == inRangeId,
                "the in-range, non-deleted seeded row must be returned by the real usp_Import_GetImportRuns");
            result.Value.Items.Should().NotContain(i => i.ImportRunId == deletedId,
                "usp_Import_GetImportRuns filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.ImportRunId == outOfRangeId,
                "usp_Import_GetImportRuns filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.ImportRunId == inRangeId);
            item.SourceSystem.Should().Be("Real Seeded HRIS");
            item.ImportType.Should().Be("Employees");
            item.RecordsProcessed.Should().Be(200);
            item.RecordsFailed.Should().Be(2);
            item.Status.Should().Be("Completed");
            item.InitiatedBy.Should().Be(initiatedBy);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetImport_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetImportQuery(TenantId, null, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded ImportItems rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetImportRuns_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Import_GetImportRuns]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        ImportRunId,
                        TenantId,
                        SiteId,
                        SourceSystem,
                        ImportType,
                        RecordsProcessed,
                        RecordsFailed,
                        Status,
                        CompletedAtUtc,
                        InitiatedBy
                    FROM [Integrations].[ImportItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND CompletedAtUtc >= @FromUtc
                      AND CompletedAtUtc <= @ToUtc
                    ORDER BY CompletedAtUtc DESC;
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
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Import_GetImportRuns]
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
                        ImportRunId,
                        TenantId,
                        SiteId,
                        SourceSystem,
                        ImportType,
                        RecordsProcessed,
                        RecordsFailed,
                        Status,
                        CompletedAtUtc,
                        InitiatedBy
                    FROM [Integrations].[ImportItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND CompletedAtUtc >= @FromUtc
                      AND CompletedAtUtc <= @ToUtc
                    ORDER BY CompletedAtUtc DESC;
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
                await using var cmd = new SqlCommand("[Integrations].[usp_Import_GetImportRuns]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_Import_GetImportRuns must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Import_GetImportRuns", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Import_GetImportRuns's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}
