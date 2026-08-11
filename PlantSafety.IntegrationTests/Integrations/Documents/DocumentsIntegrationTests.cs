// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Integrations.Documents
//  Layer:          IntegrationTests
//  File:           DocumentsIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  UPDATED:        2026-07-28 (Claude Code migration-commander-agent) -- REPLACES the prior
//                  vacuous generic outbox-insert stub (which never called the real
//                  repository/procedure) with real Phase 3 SQL Modernization coverage for the
//                  live, read-only Integrations.Documents path (IDocumentsRepository ->
//                  DocumentsDapperRepository -> [Integrations].[DocumentsItems] /
//                  usp_Documents_GetDocuments), consumed by GetDocumentsQueryHandler. This path
//                  has no write procedure, so ROWVERSION/ETag modernization does not apply here.
//  SchemaVersion:  2026.1
//  Description:    Exercises the real MediatR handler -> real repository -> real DI -> real
//                  database end to end, and forces a genuine SQL-level failure to prove the
//                  BEGIN TRY/BEGIN CATCH + ErrorLog + THROW modernization is wired up.
// ============================================================================
using MediatR;

using PlantSafety.Application.Integrations.Documents.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Integrations.Documents;

/// <summary>Integration tests for the <c>Documents</c> module in the <c>Integrations</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class DocumentsIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetDocuments_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
    {
        var siteId = Guid.NewGuid();
        var syncedBy = Guid.NewGuid();
        var inRangeId = Guid.NewGuid();
        var deletedId = Guid.NewGuid();
        var outOfRangeId = Guid.NewGuid();

        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.OpenAsync();

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[DocumentsItems]
                    (DocumentId, TenantId, SiteId, SourceSystem, FileName, DocumentType, Status, SyncedAtUtc, SyncedBy, IsDeleted)
                  VALUES (@DocumentId, @TenantId, @SiteId, @SourceSystem, @FileName, @DocumentType, @Status, @SyncedAtUtc, @SyncedBy, 0)",
                new
                {
                    DocumentId = inRangeId,
                    TenantId,
                    SiteId = siteId,
                    SourceSystem = "Real Seeded SharePoint",
                    FileName = "policy-2028.pdf",
                    DocumentType = "Policy",
                    Status = "Synced",
                    SyncedAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    SyncedBy = syncedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[DocumentsItems]
                    (DocumentId, TenantId, SiteId, SourceSystem, FileName, DocumentType, Status, SyncedAtUtc, SyncedBy, IsDeleted)
                  VALUES (@DocumentId, @TenantId, @SiteId, @SourceSystem, @FileName, @DocumentType, @Status, @SyncedAtUtc, @SyncedBy, 1)",
                new
                {
                    DocumentId = deletedId,
                    TenantId,
                    SiteId = siteId,
                    SourceSystem = "Deleted SharePoint",
                    FileName = "deleted.pdf",
                    DocumentType = "Policy",
                    Status = "Synced",
                    SyncedAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    SyncedBy = syncedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[DocumentsItems]
                    (DocumentId, TenantId, SiteId, SourceSystem, FileName, DocumentType, Status, SyncedAtUtc, SyncedBy, IsDeleted)
                  VALUES (@DocumentId, @TenantId, @SiteId, @SourceSystem, @FileName, @DocumentType, @Status, @SyncedAtUtc, @SyncedBy, 0)",
                new
                {
                    DocumentId = outOfRangeId,
                    TenantId,
                    SiteId = siteId,
                    SourceSystem = "Out Of Range SharePoint",
                    FileName = "outofrange.pdf",
                    DocumentType = "Certificate",
                    Status = "Pending",
                    SyncedAtUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    SyncedBy = syncedBy,
                });
        }

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetDocumentsQuery(TenantId, siteId, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetDocumentsQueryHandler must succeed against the real stored procedure");
            result.Value!.Items.Should().Contain(i => i.DocumentId == inRangeId,
                "the in-range, non-deleted seeded row must be returned by the real usp_Documents_GetDocuments");
            result.Value.Items.Should().NotContain(i => i.DocumentId == deletedId,
                "usp_Documents_GetDocuments filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.DocumentId == outOfRangeId,
                "usp_Documents_GetDocuments filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.DocumentId == inRangeId);
            item.SourceSystem.Should().Be("Real Seeded SharePoint");
            item.FileName.Should().Be("policy-2028.pdf");
            item.DocumentType.Should().Be("Policy");
            item.Status.Should().Be("Synced");
            item.SyncedBy.Should().Be(syncedBy);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetDocuments_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var fromUtc = new DateTime(2035, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2035, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetDocumentsQuery(TenantId, null, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded DocumentsItems rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetDocuments_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Documents_GetDocuments]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        DocumentId,
                        TenantId,
                        SiteId,
                        SourceSystem,
                        FileName,
                        DocumentType,
                        Status,
                        SyncedAtUtc,
                        SyncedBy
                    FROM [Integrations].[DocumentsItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND SyncedAtUtc >= @FromUtc
                      AND SyncedAtUtc <= @ToUtc
                    ORDER BY SyncedAtUtc DESC;
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
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Documents_GetDocuments]
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
                        DocumentId,
                        TenantId,
                        SiteId,
                        SourceSystem,
                        FileName,
                        DocumentType,
                        Status,
                        SyncedAtUtc,
                        SyncedBy
                    FROM [Integrations].[DocumentsItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND SyncedAtUtc >= @FromUtc
                      AND SyncedAtUtc <= @ToUtc
                    ORDER BY SyncedAtUtc DESC;
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
                await using var cmd = new SqlCommand("[Integrations].[usp_Documents_GetDocuments]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_Documents_GetDocuments must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Documents_GetDocuments", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Documents_GetDocuments's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}
