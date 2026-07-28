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
--  UPDATED:        2026-07-28 -- (1) added best-effort AuditLog insert on success (nested
--                  TRY/CATCH, non-fatal), matching the canonical write-procedure template and the
--                  precedent set by usp_Risk_Insert.sql / usp_CorrectiveAction_Create.sql; (2)
--                  added @ETag NVARCHAR(34) OUTPUT, projected from the row's new RowVersion
--                  column (CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)) so callers get
--                  an up-to-date ETag immediately after the idempotent check-and-insert, for both
--                  the newly-inserted and already-recorded branches; (3) wrapped the CATCH block's
--                  ErrorLog insert in its own nested TRY/CATCH per the Phase 17 reliability fix
--                  documented in 00-canonical-templates.md (an unguarded ErrorLog insert combined
--                  with SET XACT_ABORT-style doomed transactions can itself fail and mask the
--                  original business error before THROW re-raises it). Schema-only: no new
--                  conflict-rejection logic is introduced -- the idempotent check-and-insert
--                  semantics are unchanged.
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
    @Accepted      BIT OUTPUT,
    @ETag          NVARCHAR(34) OUTPUT
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        DECLARE @IsNewInsert BIT = 0;

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
            SET @IsNewInsert = 1;
        END

        SELECT @ETag = CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)
        FROM [Integrations].[InboundWebhookEvents]
        WHERE TenantId = @TenantId AND EventId = @EventId;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (
                N'InboundWebhookEvents', DB_NAME(),
                CASE WHEN @IsNewInsert = 1 THEN N'INSERT' ELSE N'IDEMPOTENT_ACCEPT' END,
                SYSTEM_USER, SYSUTCDATETIME(), 0,
                CONCAT(N'TenantId=', @TenantId, N'; EventId=', @EventId, N'; EventType=', @EventType),
                0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Audit failures must never block the primary operation.
        END CATCH;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; EventId=', @EventId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
        N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    BEGIN TRY
        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
        -- Error logging must never mask the original error.
    END CATCH;

    THROW;
END CATCH
GO
