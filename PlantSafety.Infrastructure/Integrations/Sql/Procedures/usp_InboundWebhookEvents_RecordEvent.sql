-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Commands
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_InboundWebhookEvents_RecordEvent.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Write stored procedure backing IInboundWebhookRepository.RecordEventAsync.
--                  Performs an idempotent check-and-insert against
--                  [Integrations].[InboundWebhookEvents], keyed by (TenantId, EventId): if the
--                  event has already been recorded for the tenant, the procedure returns
--                  @Accepted = 1 without inserting a duplicate row (retried deliveries of the
--                  same event are accepted, not reprocessed). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Integrations].[usp_InboundWebhookEvents_RecordEvent]
    @TenantId      UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @EventId       NVARCHAR(200),
    @EventType     NVARCHAR(200),
    @Source        NVARCHAR(200),
    @OccurredAtUtc DATETIMEOFFSET(3),
    @RawPayload    NVARCHAR(MAX),
    @Accepted      BIT OUTPUT
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        IF EXISTS (
            SELECT 1
            FROM [Integrations].[InboundWebhookEvents] WITH (UPDLOCK, HOLDLOCK)
            WHERE TenantId = @TenantId AND EventId = @EventId
        )
        BEGIN
            -- Already recorded: idempotent accept, no duplicate side effects.
            SET @Accepted = 1;
        END
        ELSE
        BEGIN
            INSERT INTO [Integrations].[InboundWebhookEvents]
                (TenantId, SiteId, EventId, EventType, Source, OccurredAtUtc, RawPayload, IsDeleted)
            VALUES
                (@TenantId, @SiteId, @EventId, @EventType, @Source, @OccurredAtUtc, @RawPayload, 0);

            SET @Accepted = 1;
        END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; EventId=', @EventId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
        N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

    THROW;
END CATCH
GO
