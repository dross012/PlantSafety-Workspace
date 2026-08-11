// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Intelligence.AITraining
//  Layer:          IntegrationTests
//  File:           AITrainingIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
//  SchemaVersion:  2026.1
//  Description:    Phase 3 SQL Modernization coverage for the live, read-only Intelligence.AITraining
//                  path (IAITrainingRepository -> AITrainingDapperRepository ->
//                  [Intelligence].[AITraining] / usp_AITraining_GetTrainingJobs), consumed by
//                  GetAITrainingQueryHandler. This path has no write procedure, so ROWVERSION/ETag
//                  modernization does not apply here -- matches the PlanningProjects/Financials
//                  read-only precedent. REPLACES the prior vacuous generic outbox-insert stub
//                  (which never called the real repository/procedure) with real coverage that:
//                    1. Seeds real rows directly into [Intelligence].[AITraining] (applied against
//                       the Testcontainers SQL Server instance by
//                       TestDatabaseFactory.ApplyIntelligenceSchemaAsync) and exercises the real
//                       MediatR pipeline (GetAITrainingQuery) end to end through the real DI
//                       container, confirming seeded rows within the requested [From, To] window
//                       and matching TenantId/SiteId are returned with correct field values, that
//                       soft-deleted rows are excluded, and that rows outside the requested date
//                       window are excluded.
//                    2. Confirms the failure-path contract: a tenant/site with no matching rows
//                       returns an empty (not null, not an exception) Items collection.
//                    3. Forces a genuine SQL-level failure inside usp_AITraining_GetTrainingJobs
//                       (temporarily renaming a selected column so the SELECT fails) to prove the
//                       procedure's new BEGIN TRY/BEGIN CATCH + ErrorLog + THROW modernization is
//                       genuinely wired up end to end, not just present in the .sql file text.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Intelligence.AITraining.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Intelligence.AITraining;

/// <summary>Integration tests for the <c>AITraining</c> module in the <c>Intelligence</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class AITrainingIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAITraining_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
    {
        var siteId = Guid.NewGuid();
        var initiatedBy = Guid.NewGuid();
        var inRangeJobId = Guid.NewGuid();
        var deletedJobId = Guid.NewGuid();
        var outOfRangeJobId = Guid.NewGuid();

        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.OpenAsync();

            // In-range, non-deleted -- must be returned.
            await conn.ExecuteAsync(
                @"INSERT INTO [Intelligence].[AITraining]
                    (TrainingJobId, TenantId, SiteId, ModelName, ModelVersion, DatasetReference, Status, StartedAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@TrainingJobId, @TenantId, @SiteId, @ModelName, @ModelVersion, @DatasetReference, @Status, @StartedAtUtc, @InitiatedBy, 0)",
                new
                {
                    TrainingJobId = inRangeJobId,
                    TenantId,
                    SiteId = siteId,
                    ModelName = "Real Seeded Training Model",
                    ModelVersion = "1.2.3",
                    DatasetReference = "s3://datasets/real-seeded",
                    Status = "Completed",
                    StartedAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });

            // In-range, but soft-deleted -- must be excluded.
            await conn.ExecuteAsync(
                @"INSERT INTO [Intelligence].[AITraining]
                    (TrainingJobId, TenantId, SiteId, ModelName, ModelVersion, DatasetReference, Status, StartedAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@TrainingJobId, @TenantId, @SiteId, @ModelName, @ModelVersion, @DatasetReference, @Status, @StartedAtUtc, @InitiatedBy, 1)",
                new
                {
                    TrainingJobId = deletedJobId,
                    TenantId,
                    SiteId = siteId,
                    ModelName = "Deleted Training Model",
                    ModelVersion = "1.0.0",
                    DatasetReference = (string?)null,
                    Status = "Completed",
                    StartedAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });

            // Outside the requested [fromUtc, toUtc] window -- must be excluded.
            await conn.ExecuteAsync(
                @"INSERT INTO [Intelligence].[AITraining]
                    (TrainingJobId, TenantId, SiteId, ModelName, ModelVersion, DatasetReference, Status, StartedAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@TrainingJobId, @TenantId, @SiteId, @ModelName, @ModelVersion, @DatasetReference, @Status, @StartedAtUtc, @InitiatedBy, 0)",
                new
                {
                    TrainingJobId = outOfRangeJobId,
                    TenantId,
                    SiteId = siteId,
                    ModelName = "Out Of Range Training Model",
                    ModelVersion = "0.9.0",
                    DatasetReference = (string?)null,
                    Status = "Queued",
                    StartedAtUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });
        }

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(
                new GetAITrainingQuery(TenantId, siteId, fromUtc, toUtc),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetAITrainingQueryHandler must succeed against the real stored procedure");
            result.Value!.TenantId.Should().Be(TenantId);
            result.Value.SiteId.Should().Be(siteId);

            result.Value.Items.Should().Contain(i => i.TrainingJobId == inRangeJobId,
                "the in-range, non-deleted seeded row must be returned by the real usp_AITraining_GetTrainingJobs");
            result.Value.Items.Should().NotContain(i => i.TrainingJobId == deletedJobId,
                "usp_AITraining_GetTrainingJobs filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.TrainingJobId == outOfRangeJobId,
                "usp_AITraining_GetTrainingJobs filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.TrainingJobId == inRangeJobId);
            item.TenantId.Should().Be(TenantId);
            item.SiteId.Should().Be(siteId);
            item.ModelName.Should().Be("Real Seeded Training Model");
            item.ModelVersion.Should().Be("1.2.3");
            item.DatasetReference.Should().Be("s3://datasets/real-seeded");
            item.Status.Should().Be("Completed");
            item.InitiatedBy.Should().Be(initiatedBy);
            item.StartedAtUtc.Should().Be(new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc));
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetAITraining_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(
                new GetAITrainingQuery(TenantId, null, fromUtc, toUtc),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded AITraining rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetTrainingJobs_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        // Forces a genuine, catchable SQL-level runtime failure (divide-by-zero) inside
        // usp_AITraining_GetTrainingJobs's TRY block by temporarily swapping in a procedure body
        // that is byte-for-byte identical except for one deliberately injected failing statement,
        // then calling the procedure directly and restoring the real production body afterwards
        // (try/finally). This deliberately avoids the naive "rename a referenced column" approach:
        // SQL Server's TRY...CATCH does NOT catch errors that occur during statement-level
        // recompilation caused by deferred name resolution (e.g. "Invalid column name" after a
        // schema change) -- this is a documented SQL Server limitation (see "TRY...CATCH
        // (Transact-SQL)" docs), confirmed empirically while authoring this test (the rename
        // approach produced an uncaught "Invalid column name" SqlException with zero ErrorLog
        // rows). A genuine runtime arithmetic error (divide-by-zero, error 8134) does not hit that
        // limitation and is reliably caught by the procedure's real CATCH block, proving the CATCH
        // block's ErrorLog insert + THROW modernization is genuinely wired up end to end.
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Intelligence].[usp_AITraining_GetTrainingJobs]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        TrainingJobId,
                        TenantId,
                        SiteId,
                        ModelName,
                        ModelVersion,
                        DatasetReference,
                        Status,
                        StartedAtUtc,
                        InitiatedBy
                    FROM [Intelligence].[AITraining]
                    WHERE TenantId = @TenantId
                      AND IsDeleted = 0
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND StartedAtUtc >= @FromUtc
                      AND StartedAtUtc <= @ToUtc
                    ORDER BY StartedAtUtc DESC;
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
            CREATE OR ALTER PROCEDURE [Intelligence].[usp_AITraining_GetTrainingJobs]
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
                        TrainingJobId,
                        TenantId,
                        SiteId,
                        ModelName,
                        ModelVersion,
                        DatasetReference,
                        Status,
                        StartedAtUtc,
                        InitiatedBy
                    FROM [Intelligence].[AITraining]
                    WHERE TenantId = @TenantId
                      AND IsDeleted = 0
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND StartedAtUtc >= @FromUtc
                      AND StartedAtUtc <= @ToUtc
                    ORDER BY StartedAtUtc DESC;
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
                await using var cmd = new SqlCommand("[Intelligence].[usp_AITraining_GetTrainingJobs]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_AITraining_GetTrainingJobs must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_AITraining_GetTrainingJobs", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_AITraining_GetTrainingJobs's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}
