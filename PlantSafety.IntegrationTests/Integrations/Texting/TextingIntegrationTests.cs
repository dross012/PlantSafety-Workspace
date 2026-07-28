// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Integrations.Texting
//  Layer:          IntegrationTests
//  File:           TextingIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  UPDATED:        2026-07-28 (Claude Code migration-commander-agent) -- REPLACES the prior
//                  vacuous generic outbox-insert stub with real Phase 3 SQL Modernization
//                  coverage for the live, read-only root-level Integrations.Texting path
//                  (ITextingRepository -> TextingDapperRepository -> [Integrations].[TextingItems]
//                  / usp_Texting_GetTextMessages), consumed by GetTextingQueryHandler. Distinct
//                  from Integrations.Notifications.Texting, covered separately in
//                  NotificationsIntegrationTests.cs. This path has no write procedure, so
//                  ROWVERSION/ETag modernization does not apply here.
//  SchemaVersion:  2026.1
//  Description:    Exercises the real MediatR handler -> real repository -> real DI -> real
//                  database end to end, and forces a genuine SQL-level failure to prove the
//                  BEGIN TRY/BEGIN CATCH + ErrorLog + THROW modernization is wired up.
// ============================================================================
using MediatR;

using PlantSafety.Application.Integrations.Texting.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Integrations.Texting;

/// <summary>Integration tests for the root-level <c>Texting</c> module in the <c>Integrations</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class TextingIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetTexting_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
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
                @"INSERT INTO [Integrations].[TextingItems]
                    (MessageId, TenantId, SiteId, RecipientPhoneNumber, MessageTemplate, GatewayProvider, Status, SentAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@MessageId, @TenantId, @SiteId, @RecipientPhoneNumber, @MessageTemplate, @GatewayProvider, @Status, @SentAtUtc, @InitiatedBy, 0)",
                new
                {
                    MessageId = inRangeId,
                    TenantId,
                    SiteId = siteId,
                    RecipientPhoneNumber = "+15551230001",
                    MessageTemplate = "IncidentAlert",
                    GatewayProvider = "Twilio",
                    Status = "Delivered",
                    SentAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[TextingItems]
                    (MessageId, TenantId, SiteId, RecipientPhoneNumber, MessageTemplate, GatewayProvider, Status, SentAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@MessageId, @TenantId, @SiteId, @RecipientPhoneNumber, @MessageTemplate, @GatewayProvider, @Status, @SentAtUtc, @InitiatedBy, 1)",
                new
                {
                    MessageId = deletedId,
                    TenantId,
                    SiteId = siteId,
                    RecipientPhoneNumber = "+15551230002",
                    MessageTemplate = "ShiftReminder",
                    GatewayProvider = "Twilio",
                    Status = "Delivered",
                    SentAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[TextingItems]
                    (MessageId, TenantId, SiteId, RecipientPhoneNumber, MessageTemplate, GatewayProvider, Status, SentAtUtc, InitiatedBy, IsDeleted)
                  VALUES (@MessageId, @TenantId, @SiteId, @RecipientPhoneNumber, @MessageTemplate, @GatewayProvider, @Status, @SentAtUtc, @InitiatedBy, 0)",
                new
                {
                    MessageId = outOfRangeId,
                    TenantId,
                    SiteId = siteId,
                    RecipientPhoneNumber = "+15551230003",
                    MessageTemplate = "IncidentAlert",
                    GatewayProvider = "Twilio",
                    Status = "Failed",
                    SentAtUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    InitiatedBy = initiatedBy,
                });
        }

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new GetTextingQuery(TenantId, siteId, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetTextingQueryHandler must succeed against the real stored procedure");
            result.Value!.Items.Should().Contain(i => i.MessageId == inRangeId,
                "the in-range, non-deleted seeded row must be returned by the real usp_Texting_GetTextMessages");
            result.Value.Items.Should().NotContain(i => i.MessageId == deletedId,
                "usp_Texting_GetTextMessages filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.MessageId == outOfRangeId,
                "usp_Texting_GetTextMessages filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.MessageId == inRangeId);
            item.RecipientPhoneNumber.Should().Be("+15551230001");
            item.MessageTemplate.Should().Be("IncidentAlert");
            item.GatewayProvider.Should().Be("Twilio");
            item.Status.Should().Be("Delivered");
            item.InitiatedBy.Should().Be(initiatedBy);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetTexting_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var noDataTenantId = Guid.NewGuid();
        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new GetTextingQuery(noDataTenantId, null, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded TextingItems rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetTextMessages_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Texting_GetTextMessages]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        MessageId,
                        TenantId,
                        SiteId,
                        RecipientPhoneNumber,
                        MessageTemplate,
                        GatewayProvider,
                        Status,
                        SentAtUtc,
                        InitiatedBy
                    FROM [Integrations].[TextingItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND SentAtUtc >= @FromUtc
                      AND SentAtUtc <= @ToUtc
                    ORDER BY SentAtUtc DESC;
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
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Texting_GetTextMessages]
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
                        MessageId,
                        TenantId,
                        SiteId,
                        RecipientPhoneNumber,
                        MessageTemplate,
                        GatewayProvider,
                        Status,
                        SentAtUtc,
                        InitiatedBy
                    FROM [Integrations].[TextingItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND SentAtUtc >= @FromUtc
                      AND SentAtUtc <= @ToUtc
                    ORDER BY SentAtUtc DESC;
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
                await using var cmd = new SqlCommand("[Integrations].[usp_Texting_GetTextMessages]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_Texting_GetTextMessages must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Texting_GetTextMessages", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Texting_GetTextMessages's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}
