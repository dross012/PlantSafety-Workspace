// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Integrations.Notifications
//  Layer:          IntegrationTests
//  File:           NotificationsIntegrationTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  UPDATED:        2026-07-28 (Claude Code migration-commander-agent) -- REPLACES the prior
//                  vacuous generic outbox-insert stub with real Phase 3 SQL Modernization
//                  coverage for all three read-only Integrations.Notifications.* paths, grouped
//                  into one file since they naturally group under the shared Notifications area:
//                    - Notifications.Email    (IEmailRepository    -> NotificationsEmailDapperRepository
//                                               -> [Integrations].[NotificationsEmailItems] /
//                                                  usp_NotificationsEmail_GetEmailDeliveries)
//                    - Notifications.Internal (IInternalRepository -> InternalDapperRepository
//                                               -> [Integrations].[NotificationsInternalItems] /
//                                                  usp_NotificationsInternal_GetInternalNotifications)
//                    - Notifications.Texting  (ITextingRepository  -> NotificationsTextingDapperRepository
//                                               -> [Integrations].[NotificationsTextingItems] /
//                                                  usp_NotificationsTexting_GetTextingDeliveries)
//                  None of these three has a write procedure, so ROWVERSION/ETag modernization
//                  does not apply to any of them.
//  SchemaVersion:  2026.1
//  Description:    Each test class exercises the real MediatR handler -> real repository -> real
//                  DI -> real database end to end, and forces a genuine SQL-level failure to prove
//                  the BEGIN TRY/BEGIN CATCH + ErrorLog + THROW modernization is wired up.
// ============================================================================
using MediatR;

using PlantSafety.Application.Integrations.Notifications.Email.Queries;
using PlantSafety.Application.Integrations.Notifications.Internal.Queries;
using PlantSafety.Application.Integrations.Notifications.Texting.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Integrations.Notifications;

/// <summary>Integration tests for the <c>Notifications.Email</c> module in the <c>Integrations</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class NotificationsEmailIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetEmailDeliveries_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
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
                @"INSERT INTO [Integrations].[NotificationsEmailItems]
                    (EmailDeliveryId, TenantId, SiteId, RecipientAddress, Subject, TemplateName, Status, SentAtUtc, SentBy, IsDeleted)
                  VALUES (@EmailDeliveryId, @TenantId, @SiteId, @RecipientAddress, @Subject, @TemplateName, @Status, @SentAtUtc, @SentBy, 0)",
                new
                {
                    EmailDeliveryId = inRangeId,
                    TenantId,
                    SiteId = siteId,
                    RecipientAddress = "real.seeded@example.com",
                    Subject = "Real Seeded Notification",
                    TemplateName = "IncidentDigest",
                    Status = "Delivered",
                    SentAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    SentBy = sentBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[NotificationsEmailItems]
                    (EmailDeliveryId, TenantId, SiteId, RecipientAddress, Subject, TemplateName, Status, SentAtUtc, SentBy, IsDeleted)
                  VALUES (@EmailDeliveryId, @TenantId, @SiteId, @RecipientAddress, @Subject, @TemplateName, @Status, @SentAtUtc, @SentBy, 1)",
                new
                {
                    EmailDeliveryId = deletedId,
                    TenantId,
                    SiteId = siteId,
                    RecipientAddress = "deleted@example.com",
                    Subject = "Deleted Notification",
                    TemplateName = (string?)null,
                    Status = "Delivered",
                    SentAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    SentBy = sentBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[NotificationsEmailItems]
                    (EmailDeliveryId, TenantId, SiteId, RecipientAddress, Subject, TemplateName, Status, SentAtUtc, SentBy, IsDeleted)
                  VALUES (@EmailDeliveryId, @TenantId, @SiteId, @RecipientAddress, @Subject, @TemplateName, @Status, @SentAtUtc, @SentBy, 0)",
                new
                {
                    EmailDeliveryId = outOfRangeId,
                    TenantId,
                    SiteId = siteId,
                    RecipientAddress = "outofrange@example.com",
                    Subject = "Out Of Range Notification",
                    TemplateName = (string?)null,
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

            result.IsSuccess.Should().BeTrue("the real GetEmailQueryHandler (Notifications.Email) must succeed against the real stored procedure");
            result.Value!.Items.Should().Contain(i => i.EmailDeliveryId == inRangeId,
                "the in-range, non-deleted seeded row must be returned by the real usp_NotificationsEmail_GetEmailDeliveries");
            result.Value.Items.Should().NotContain(i => i.EmailDeliveryId == deletedId,
                "usp_NotificationsEmail_GetEmailDeliveries filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.EmailDeliveryId == outOfRangeId,
                "usp_NotificationsEmail_GetEmailDeliveries filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.EmailDeliveryId == inRangeId);
            item.RecipientAddress.Should().Be("real.seeded@example.com");
            item.Subject.Should().Be("Real Seeded Notification");
            item.TemplateName.Should().Be("IncidentDigest");
            item.Status.Should().Be("Delivered");
            item.SentBy.Should().Be(sentBy);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetEmailDeliveries_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetEmailQuery(TenantId, null, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded NotificationsEmailItems rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetEmailDeliveries_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_NotificationsEmail_GetEmailDeliveries]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        EmailDeliveryId,
                        TenantId,
                        SiteId,
                        RecipientAddress,
                        Subject,
                        TemplateName,
                        Status,
                        SentAtUtc,
                        SentBy
                    FROM [Integrations].[NotificationsEmailItems]
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
            CREATE OR ALTER PROCEDURE [Integrations].[usp_NotificationsEmail_GetEmailDeliveries]
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
                        EmailDeliveryId,
                        TenantId,
                        SiteId,
                        RecipientAddress,
                        Subject,
                        TemplateName,
                        Status,
                        SentAtUtc,
                        SentBy
                    FROM [Integrations].[NotificationsEmailItems]
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
                await using var cmd = new SqlCommand("[Integrations].[usp_NotificationsEmail_GetEmailDeliveries]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_NotificationsEmail_GetEmailDeliveries must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_NotificationsEmail_GetEmailDeliveries", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_NotificationsEmail_GetEmailDeliveries's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}

/// <summary>Integration tests for the <c>Notifications.Internal</c> module in the <c>Integrations</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class NotificationsInternalIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetInternalNotifications_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
    {
        var siteId = Guid.NewGuid();
        var recipientUserId = Guid.NewGuid();
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
                @"INSERT INTO [Integrations].[NotificationsInternalItems]
                    (NotificationId, TenantId, SiteId, RecipientUserId, Message, Category, Status, DeliveredAtUtc, TriggeredBy, IsDeleted)
                  VALUES (@NotificationId, @TenantId, @SiteId, @RecipientUserId, @Message, @Category, @Status, @DeliveredAtUtc, @TriggeredBy, 0)",
                new
                {
                    NotificationId = inRangeId,
                    TenantId,
                    SiteId = siteId,
                    RecipientUserId = recipientUserId,
                    Message = "Real seeded in-app notification",
                    Category = "Alert",
                    Status = "Unread",
                    DeliveredAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    TriggeredBy = triggeredBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[NotificationsInternalItems]
                    (NotificationId, TenantId, SiteId, RecipientUserId, Message, Category, Status, DeliveredAtUtc, TriggeredBy, IsDeleted)
                  VALUES (@NotificationId, @TenantId, @SiteId, @RecipientUserId, @Message, @Category, @Status, @DeliveredAtUtc, @TriggeredBy, 1)",
                new
                {
                    NotificationId = deletedId,
                    TenantId,
                    SiteId = siteId,
                    RecipientUserId = recipientUserId,
                    Message = "Deleted notification",
                    Category = (string?)null,
                    Status = "Read",
                    DeliveredAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    TriggeredBy = triggeredBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[NotificationsInternalItems]
                    (NotificationId, TenantId, SiteId, RecipientUserId, Message, Category, Status, DeliveredAtUtc, TriggeredBy, IsDeleted)
                  VALUES (@NotificationId, @TenantId, @SiteId, @RecipientUserId, @Message, @Category, @Status, @DeliveredAtUtc, @TriggeredBy, 0)",
                new
                {
                    NotificationId = outOfRangeId,
                    TenantId,
                    SiteId = siteId,
                    RecipientUserId = recipientUserId,
                    Message = "Out of range notification",
                    Category = (string?)null,
                    Status = "Dismissed",
                    DeliveredAtUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    TriggeredBy = triggeredBy,
                });
        }

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetInternalQuery(TenantId, siteId, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetInternalQueryHandler must succeed against the real stored procedure");
            result.Value!.Items.Should().Contain(i => i.NotificationId == inRangeId,
                "the in-range, non-deleted seeded row must be returned by the real usp_NotificationsInternal_GetInternalNotifications");
            result.Value.Items.Should().NotContain(i => i.NotificationId == deletedId,
                "usp_NotificationsInternal_GetInternalNotifications filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.NotificationId == outOfRangeId,
                "usp_NotificationsInternal_GetInternalNotifications filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.NotificationId == inRangeId);
            item.RecipientUserId.Should().Be(recipientUserId);
            item.Message.Should().Be("Real seeded in-app notification");
            item.Category.Should().Be("Alert");
            item.Status.Should().Be("Unread");
            item.TriggeredBy.Should().Be(triggeredBy);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetInternalNotifications_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetInternalQuery(TenantId, null, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded NotificationsInternalItems rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetInternalNotifications_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_NotificationsInternal_GetInternalNotifications]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        NotificationId,
                        TenantId,
                        SiteId,
                        RecipientUserId,
                        Message,
                        Category,
                        Status,
                        DeliveredAtUtc,
                        TriggeredBy
                    FROM [Integrations].[NotificationsInternalItems]
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
            CREATE OR ALTER PROCEDURE [Integrations].[usp_NotificationsInternal_GetInternalNotifications]
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
                        NotificationId,
                        TenantId,
                        SiteId,
                        RecipientUserId,
                        Message,
                        Category,
                        Status,
                        DeliveredAtUtc,
                        TriggeredBy
                    FROM [Integrations].[NotificationsInternalItems]
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
                await using var cmd = new SqlCommand("[Integrations].[usp_NotificationsInternal_GetInternalNotifications]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_NotificationsInternal_GetInternalNotifications must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_NotificationsInternal_GetInternalNotifications", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_NotificationsInternal_GetInternalNotifications's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}

/// <summary>Integration tests for the <c>Notifications.Texting</c> module in the <c>Integrations</c> area.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class NotificationsTextingIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetTextingDeliveries_ShouldReturnSeededRealDataWithinRange_AndExcludeDeletedAndOutOfRange()
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
                @"INSERT INTO [Integrations].[NotificationsTextingItems]
                    (SmsDeliveryId, TenantId, SiteId, RecipientPhoneNumber, MessageBody, TriggerEvent, Status, SentAtUtc, SentBy, IsDeleted)
                  VALUES (@SmsDeliveryId, @TenantId, @SiteId, @RecipientPhoneNumber, @MessageBody, @TriggerEvent, @Status, @SentAtUtc, @SentBy, 0)",
                new
                {
                    SmsDeliveryId = inRangeId,
                    TenantId,
                    SiteId = siteId,
                    RecipientPhoneNumber = "+15551240001",
                    MessageBody = "Real seeded SMS notification body",
                    TriggerEvent = "IncidentCreated",
                    Status = "Delivered",
                    SentAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    SentBy = sentBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[NotificationsTextingItems]
                    (SmsDeliveryId, TenantId, SiteId, RecipientPhoneNumber, MessageBody, TriggerEvent, Status, SentAtUtc, SentBy, IsDeleted)
                  VALUES (@SmsDeliveryId, @TenantId, @SiteId, @RecipientPhoneNumber, @MessageBody, @TriggerEvent, @Status, @SentAtUtc, @SentBy, 1)",
                new
                {
                    SmsDeliveryId = deletedId,
                    TenantId,
                    SiteId = siteId,
                    RecipientPhoneNumber = "+15551240002",
                    MessageBody = "Deleted SMS notification body",
                    TriggerEvent = (string?)null,
                    Status = "Delivered",
                    SentAtUtc = new DateTime(2028, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    SentBy = sentBy,
                });

            await conn.ExecuteAsync(
                @"INSERT INTO [Integrations].[NotificationsTextingItems]
                    (SmsDeliveryId, TenantId, SiteId, RecipientPhoneNumber, MessageBody, TriggerEvent, Status, SentAtUtc, SentBy, IsDeleted)
                  VALUES (@SmsDeliveryId, @TenantId, @SiteId, @RecipientPhoneNumber, @MessageBody, @TriggerEvent, @Status, @SentAtUtc, @SentBy, 0)",
                new
                {
                    SmsDeliveryId = outOfRangeId,
                    TenantId,
                    SiteId = siteId,
                    RecipientPhoneNumber = "+15551240003",
                    MessageBody = "Out of range SMS notification body",
                    TriggerEvent = (string?)null,
                    Status = "Failed",
                    SentAtUtc = new DateTime(2029, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    SentBy = sentBy,
                });
        }

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetTextingQuery(TenantId, siteId, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("the real GetTextingQueryHandler (Notifications.Texting) must succeed against the real stored procedure");
            result.Value!.Items.Should().Contain(i => i.SmsDeliveryId == inRangeId,
                "the in-range, non-deleted seeded row must be returned by the real usp_NotificationsTexting_GetTextingDeliveries");
            result.Value.Items.Should().NotContain(i => i.SmsDeliveryId == deletedId,
                "usp_NotificationsTexting_GetTextingDeliveries filters IsDeleted = 1 rows");
            result.Value.Items.Should().NotContain(i => i.SmsDeliveryId == outOfRangeId,
                "usp_NotificationsTexting_GetTextingDeliveries filters rows outside [@FromUtc, @ToUtc]");

            var item = result.Value.Items.Single(i => i.SmsDeliveryId == inRangeId);
            item.RecipientPhoneNumber.Should().Be("+15551240001");
            item.MessageBody.Should().Be("Real seeded SMS notification body");
            item.TriggerEvent.Should().Be("IncidentCreated");
            item.Status.Should().Be("Delivered");
            item.SentBy.Should().Be(sentBy);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetTextingDeliveries_NoMatchingRows_ShouldReturnEmptyItems_NotExceptionOrNull()
    {
        var fromUtc = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var scope = TenantHttpContextTestHelper.CreateScopeWithResolvedTenant(Factory, TenantId);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        try
        {
            var result = await sender.Send(new GetTextingQuery(TenantId, null, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue("a query with no matching rows must still succeed, not throw");
            result.Value!.Items.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty("a tenant/site with no seeded NotificationsTextingItems rows must yield an empty list, not an exception");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetTextingDeliveries_ForcedSqlFailure_ShouldThrowAndPopulateErrorLog()
    {
        var tenantId = Guid.NewGuid();

        await using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string originalProcedure = @"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_NotificationsTexting_GetTextingDeliveries]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        SmsDeliveryId,
                        TenantId,
                        SiteId,
                        RecipientPhoneNumber,
                        MessageBody,
                        TriggerEvent,
                        Status,
                        SentAtUtc,
                        SentBy
                    FROM [Integrations].[NotificationsTextingItems]
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
            CREATE OR ALTER PROCEDURE [Integrations].[usp_NotificationsTexting_GetTextingDeliveries]
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
                        SmsDeliveryId,
                        TenantId,
                        SiteId,
                        RecipientPhoneNumber,
                        MessageBody,
                        TriggerEvent,
                        Status,
                        SentAtUtc,
                        SentBy
                    FROM [Integrations].[NotificationsTextingItems]
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
                await using var cmd = new SqlCommand("[Integrations].[usp_NotificationsTexting_GetTextingDeliveries]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@SiteId", DBNull.Value);
                cmd.Parameters.AddWithValue("@FromUtc", new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                cmd.Parameters.AddWithValue("@ToUtc", new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc));

                await cmd.ExecuteReaderAsync();
            };

            await act.Should().ThrowAsync<SqlException>(
                "usp_NotificationsTexting_GetTextingDeliveries must re-throw (THROW) after logging to ErrorLog");
        }
        finally
        {
            await conn.ExecuteAsync(originalProcedure);
        }

        var errorLogCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.ErrorLog WHERE ErrorProcedure LIKE @ErrorProcedurePattern AND ErrorContext LIKE @Pattern",
            new { ErrorProcedurePattern = "%" + "usp_NotificationsTexting_GetTextingDeliveries", Pattern = $"%TenantId={tenantId}%" });

        errorLogCount.Should().BeGreaterThan(0,
            "usp_NotificationsTexting_GetTextingDeliveries's CATCH block must have logged the forced failure to ErrorLog before re-throwing");
    }
}
