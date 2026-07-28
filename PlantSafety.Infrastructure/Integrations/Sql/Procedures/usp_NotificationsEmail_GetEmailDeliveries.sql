-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Notifications.Email
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_NotificationsEmail_GetEmailDeliveries.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  Integrations.Notifications.Email.IEmailRepository.GetEmailDeliveriesAsync.
--                  Additive-only evolution.
--  UPDATED:        2026-07-28 -- wrapped in BEGIN TRY/BEGIN CATCH with a canonical ErrorLog
--                  insert (ErrorProcedure/ErrorLine/ErrorMessage/ErrorContext/... shape matching
--                  every write procedure's CATCH block, e.g. usp_InboundWebhookEvents_RecordEvent.sql)
--                  followed by THROW to re-raise. Deliberate, explicit exception to this codebase's
--                  normal convention (read-only Get/List procs are intentionally TRY-free
--                  elsewhere) -- no AuditLog, no ROWVERSION/ETag: this table has no write path.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

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

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
