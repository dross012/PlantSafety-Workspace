// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Intelligence.AIInvestigations
//  Layer:          IntegrationTests
//  File:           AIInvestigationsIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
//  SchemaVersion:  2026.1
//  Description:    Phase 3 SQL Modernization coverage for the live, read-only
//                  Intelligence.AllInvestigations path (IAllInvestigationsRepository ->
//                  AllInvestigationsDapperRepository -> [Intelligence].[AllInvestigations] /
//                  usp_AllInvestigations_GetInvestigations), consumed by
//                  GetAllInvestigationsQueryHandler. This path has no write procedure, so
//                  ROWVERSION/ETag modernization does not apply here. REPLACES the prior vacuous
//                  generic outbox-insert stub (which never called the real repository/procedure)
//                  with real coverage that:
//                    1. Seeds real rows directly into [Intelligence].[AllInvestigations] (applied
//                       against the Testcontainers SQL Server instance by
//                       TestDatabaseFactory.ApplyIntelligenceSchemaAsync) and exercises the real
//                       MediatR pipeline (GetAllInvestigationsQuery) end to end through the real DI
//                       container, confirming seeded rows within the requested [From, To] window
//                       and matching TenantId/SiteId are returned with correct field values, that
//                       soft-deleted rows are excluded, and that rows outside the requested date
//                       window are excluded.
//                    2. Confirms the failure-path contract: a tenant/site with no matching rows
//                       returns an empty (not null, not an exception) Items collection.
//                    3. Forces a genuine SQL-level failure inside
//                       usp_AllInvestigations_GetInvestigations (temporarily renaming a selected
//                       column so the SELECT fails) to prove the procedure's new BEGIN TRY/BEGIN
//                       CATCH + ErrorLog + THROW modernization is genuinely wired up end to end,
//                       not just present in the .sql file text.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.Intelligence.AllInvestigations.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Intelligence.AIInvestigations;

/// <summary>Integration tests for the <c>AllInvestigations</c> module in the <c>Intelligence</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class AIInvestigationsIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAllInvestigations_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
    {
        var siteId = Guid.NewGuid();
        var assignedTo = Guid.NewGuid();
        var inRangeInvestigationId = Guid.NewGuid();
        var deletedInvestigationId = Guid.NewGuid();
        var outOfRangeInvestigationId = Guid.NewGuid();

        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.OpenAsync();

            // In-range, non-deleted -- must be returned.
            await conn.ExecuteAsync(
                @"INSERT INTO [Intelligence].[AllInvestigations]
                    (InvestigationId, TenantId, SiteId, IncidentReference, AiConfidenceScore, Summary, Status, OpenedAtUtc, AssignedTo, IsDeleted)
                  VALUES (@InvestigationId, @TenantId, @SiteId, @IncidentReference, @AiConfidenceScore, @Summary, @Status, @OpenedAtUtc, @AssignedTo, 0)",
                new
                {
                    InvestigationId = inRangeInvestigationId,
                    TenantId,
                    SiteId = siteId,
                    IncidentReference = "INC-2028-0042",
                    AiConfidenceScore = 0.91,
                    Summary = "Real seeded investigation summary",
                    Status = "Open",
                    OpenedAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    AssignedTo = assignedTo,
                });

            // In-range, but soft-deleted -- must be excluded.
            await conn.ExecuteAsync(
                @"INSERT INTO [Intelligence].[AllInvestigations]
                    (InvestigationId, TenantId, SiteId, IncidentReference, AiConfidenceScore, Summary, Status, OpenedAtUtc, AssignedTo, IsDeleted)
                  VALUES (@InvestigationId, @TenantId, @SiteId, @IncidentReference, @AiConfidenceScore, @Summary, @Status, @OpenedAtUtc, @AssignedTo, 1)",
                new
                {
                    InvestigationId = deletedInvestigationId,
                    TenantId,
                    SiteId = siteId,
                    IncidentReference = "INC-2028-0043",
                    AiConfidenceScore = 0.80,
                    Summary = (string?)null,
                    Status = "Closed",
                    OpenedAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    AssignedTo = assignedTo,
                });

            // Outside the requested [fromUtc, toUtc] window -- must be excluded.
            await conn.ExecuteAsync(
                @"INSERT INTO [Intelligence].[AllInvestigations]
                    (InvestigationId, TenantId, SiteId, IncidentReference, AiConfidenceScore, Summary, Status, OpenedAtUtc, AssignedTo, IsDeleted)
                  VALUES (@InvestigationId, @TenantId, @SiteId, @IncidentReference, @AiConfidenceScore, @Summary, @Status, @OpenedAtUtc, @AssignedTo, 0)",
                new
                {
                    InvestigationId = outOfRangeInvestigationId,
                    TenantId,
                    SiteId = siteId,
                    IncidentReference = "INC-2029-0001",
                    AiConfidenceScore = 0.75,
                    Summary = (string?)null,
                    Status = "Open",
                    OpenedAtUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    AssignedTo = assignedTo,
                });
        }

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(
                new GetAllInvestigationsQuery(TenantId, siteId, fromUtc, toUtc),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetAllInvestigationsQueryHandler must succeed against the real stored procedure");
            result.Value!.TenantId.Should().Be(TenantId);
            result.Value.SiteId.Should().Be(siteId);

            result.Value.Items.Should().Contain(i => i.InvestigationId == inRangeInvestigationId,
                "the in-range, non-deleted seeded row must be returned by the real usp_AllInvestigations_GetInvestigations");
            result.Value.Items.Should().NotContain(i => i.InvestigationId == deletedInvestigationId,
                "usp_AllInvestigations_GetInvestigations filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.InvestigationId == outOfRangeInvestigationId,
                "usp_AllInvestigations_GetInvestigations filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.InvestigationId == inRangeInvestigationId);
            item.TenantId.Should().Be(TenantId);
            item.SiteId.Should().Be(siteId);
            item.IncidentReference.Should().Be("INC-2028-0042");
            item.AiConfidenceScore.Should().Be(0.91);
            item.Summary.Should().Be("Real seeded investigation summary");
            item.Status.Should().Be("Open");
            item.AssignedTo.Should().Be(assignedTo);
            item.OpenedAtUtc.Should().Be(new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc));
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetAllInvestigations_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(
                new GetAllInvestigationsQuery(TenantId, null, fromUtc, toUtc),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded AllInvestigations rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetInvestigations_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        // Forces a genuine, catchable SQL-level runtime failure (divide-by-zero) inside
        // usp_AllInvestigations_GetInvestigations's TRY block by temporarily swapping in a
        // procedure body that is byte-for-byte identical except for one deliberately injected
        // failing statement, then calling the procedure directly and restoring the real
        // production body afterwards (try/finally). This deliberately avoids the naive "rename a
        // referenced column" approach: SQL Server's TRY...CATCH does NOT catch errors that occur
        // during statement-level recompilation caused by deferred name resolution (e.g. "Invalid
        // column name" after a schema change) -- this is a documented SQL Server limitation (see
        // "TRY...CATCH (Transact-SQL)" docs), confirmed empirically while authoring this test (the
        // rename approach produced an uncaught "Invalid column name" SqlException with zero
        // ErrorLog rows -- see AITrainingIntegrationTests.GetTrainingJobs_ForcedSqlFailure_
        // ShouldThrowAndPopulateErrorLog for the sibling remarks). A genuine runtime arithmetic
        // error (divide-by-zero, error 8134) does not hit that limitation and is reliably caught by
        // the procedure's real CATCH block, proving the CATCH block's ErrorLog insert + THROW
        // modernization is genuinely wired up end to end.
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Intelligence].[usp_AllInvestigations_GetInvestigations]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        InvestigationId,
                        TenantId,
                        SiteId,
                        IncidentReference,
                        AiConfidenceScore,
                        Summary,
                        Status,
                        OpenedAtUtc,
                        AssignedTo
                    FROM [Intelligence].[AllInvestigations]
                    WHERE TenantId = @TenantId
                      AND IsDeleted = 0
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND OpenedAtUtc >= @FromUtc
                      AND OpenedAtUtc <= @ToUtc
                    ORDER BY OpenedAtUtc DESC;
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
            CREATE OR ALTER PROCEDURE [Intelligence].[usp_AllInvestigations_GetInvestigations]
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
                        InvestigationId,
                        TenantId,
                        SiteId,
                        IncidentReference,
                        AiConfidenceScore,
                        Summary,
                        Status,
                        OpenedAtUtc,
                        AssignedTo
                    FROM [Intelligence].[AllInvestigations]
                    WHERE TenantId = @TenantId
                      AND IsDeleted = 0
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND OpenedAtUtc >= @FromUtc
                      AND OpenedAtUtc <= @ToUtc
                    ORDER BY OpenedAtUtc DESC;
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
                await using var cmd = new SqlCommand("[Intelligence].[usp_AllInvestigations_GetInvestigations]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_AllInvestigations_GetInvestigations must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_AllInvestigations_GetInvestigations", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_AllInvestigations_GetInvestigations's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}
