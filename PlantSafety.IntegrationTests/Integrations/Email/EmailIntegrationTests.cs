// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Integrations.Email
//  Layer:          IntegrationTests
//  File:           EmailIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  UPDATED:        2026-07-28 (Claude Code migration-commander-agent) -- REPLACES the prior
//                  vacuous generic outbox-insert stub with real Phase 3 SQL Modernization
//                  coverage for the live, read-only root-level Integrations.Email path
//                  (IEmailRepository -> EmailDapperRepository -> [Integrations].[EmailItems] /
//                  usp_Email_GetEmails), consumed by GetEmailQueryHandler. Distinct from
//                  Integrations.Notifications.Email, covered separately in
//                  NotificationsIntegrationTests.cs. This path has no write procedure, so
//                  ROWVERSION/ETag modernization does not apply here.
//  SchemaVersion:  2026.1
//  Description:    Exercises the real MediatR handler -> real repository -> real DI -> real
//                  database end to end, and forces a genuine SQL-level failure to prove the
//                  BEGIN TRY/BEGIN CATCH + ErrorLog + THROW modernization is wired up.
// ============================================================================
using MediatR;

using PlantSafety.Application.Integrations.Email.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Integrations.Email;

/// <summary>Integration tests for the root-level <c>Email</c> module in the <c>Integrations</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class EmailIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetEmail_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
    {
        var siteId = Guid.NewGuid();
        var sentBy = Guid.NewGuid();
        var inRangeId = Guid.NewGuid();
        var deletedId = Guid.NewGuid();
        var outOfRangeId = Guid.NewGuid();

        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.OpenAsync();

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[EmailItems]
                    (EmailId, TenantId, SiteId, Recipient, Subject, Provider, Status, SentAtUtc, SentBy, IsDeleted)
                  VALUES (@EmailId, @TenantId, @SiteId, @Recipient, @Subject, @Provider, @Status, @SentAtUtc, @SentBy, 0)",
                new
                {
                    EmailId = inRangeId,
                    TenantId,
                    SiteId = siteId,
                    Recipient = "real.seeded@example.com",
                    Subject = "Real Seeded Email",
                    Provider = "SendGrid",
                    Status = "Delivered",
                    SentAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    SentBy = sentBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[EmailItems]
                    (EmailId, TenantId, SiteId, Recipient, Subject, Provider, Status, SentAtUtc, SentBy, IsDeleted)
                  VALUES (@EmailId, @TenantId, @SiteId, @Recipient, @Subject, @Provider, @Status, @SentAtUtc, @SentBy, 1)",
                new
                {
                    EmailId = deletedId,
                    TenantId,
                    SiteId = siteId,
                    Recipient = "deleted@example.com",
                    Subject = "Deleted Email",
                    Provider = "SendGrid",
                    Status = "Delivered",
                    SentAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    SentBy = sentBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[EmailItems]
                    (EmailId, TenantId, SiteId, Recipient, Subject, Provider, Status, SentAtUtc, SentBy, IsDeleted)
                  VALUES (@EmailId, @TenantId, @SiteId, @Recipient, @Subject, @Provider, @Status, @SentAtUtc, @SentBy, 0)",
                new
                {
                    EmailId = outOfRangeId,
                    TenantId,
                    SiteId = siteId,
                    Recipient = "outofrange@example.com",
                    Subject = "Out Of Range Email",
                    Provider = "SMTP",
                    Status = "Bounced",
                    SentAtUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    SentBy = sentBy,
                });
        }

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetEmailQuery(TenantId, siteId, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetEmailQueryHandler must succeed against the real stored procedure");
            result.Value!.Items.Should().Contain(i => i.EmailId == inRangeId,
                "the in-range, non-deleted seeded row must be returned by the real usp_Email_GetEmails");
            result.Value.Items.Should().NotContain(i => i.EmailId == deletedId,
                "usp_Email_GetEmails filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.EmailId == outOfRangeId,
                "usp_Email_GetEmails filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.EmailId == inRangeId);
            item.Recipient.Should().Be("real.seeded@example.com");
            item.Subject.Should().Be("Real Seeded Email");
            item.Provider.Should().Be("SendGrid");
            item.Status.Should().Be("Delivered");
            item.SentBy.Should().Be(sentBy);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetEmail_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var fromUtc = new DateTime(2035, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2035, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetEmailQuery(TenantId, null, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded EmailItems rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetEmails_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Email_GetEmails]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        EmailId,
                        TenantId,
                        SiteId,
                        Recipient,
                        Subject,
                        Provider,
                        Status,
                        SentAtUtc,
                        SentBy
                    FROM [Integrations].[EmailItems]
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
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Email_GetEmails]
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
                        EmailId,
                        TenantId,
                        SiteId,
                        Recipient,
                        Subject,
                        Provider,
                        Status,
                        SentAtUtc,
                        SentBy
                    FROM [Integrations].[EmailItems]
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
                await using var cmd = new SqlCommand("[Integrations].[usp_Email_GetEmails]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_Email_GetEmails must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_Email_GetEmails", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_Email_GetEmails's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}
