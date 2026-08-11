-- ============================================================================
--  Module:  Alerts.Alert | Layer: Infrastructure (SQL) | Created: 2026-07-21
--  Description: Insert procedures for the Alert aggregate's child entities:
--               AlertDeliveryRecord and AlertResponseAction.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_RecordDelivery]
    @DeliveryRecordId UNIQUEIDENTIFIER,
    @AlertId          UNIQUEIDENTIFIER,
    @TenantId         UNIQUEIDENTIFIER,
    @Channel          NVARCHAR(30),
    @Succeeded        BIT,
    @AttemptedAtUtc   DATETIME2(3),
    @FailureReason    NVARCHAR(500) = NULL,
    @CorrelationId    UNIQUEIDENTIFIER,
    @ActorId          UNIQUEIDENTIFIER,
    @NowUtc           DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        INSERT INTO [Alerts].[AlertDeliveryRecord]
            ([DeliveryRecordId], [AlertId], [TenantId], [Channel], [Succeeded], [AttemptedAtUtc], [FailureReason], [CreatedAtUtc])
        VALUES
            (@DeliveryRecordId, @AlertId, @TenantId, @Channel, @Succeeded, @AttemptedAtUtc, @FailureReason, @NowUtc);

        UPDATE [Alerts].[Alert] SET [UpdatedAtUtc] = @NowUtc WHERE [AlertId] = @AlertId AND [TenantId] = @TenantId;
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'AlertDeliveryRecord', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'DeliveryRecordId=', @DeliveryRecordId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DeliveryRecordId=', @DeliveryRecordId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_RecordResponseAction]
    @ResponseActionId  UNIQUEIDENTIFIER,
    @AlertId           UNIQUEIDENTIFIER,
    @TenantId          UNIQUEIDENTIFIER,
    @ActionDescription NVARCHAR(500),
    @TakenAtUtc        DATETIME2(3),
    @TakenByActorKind  NVARCHAR(20),
    @TakenByUserId     UNIQUEIDENTIFIER = NULL,
    @Notes             NVARCHAR(2000) = NULL,
    @CorrelationId     UNIQUEIDENTIFIER,
    @ActorId           UNIQUEIDENTIFIER,
    @NowUtc            DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        INSERT INTO [Alerts].[AlertResponseAction]
            ([ResponseActionId], [AlertId], [TenantId], [ActionDescription], [TakenAtUtc], [TakenByActorKind], [TakenByUserId], [Notes], [CreatedAtUtc])
        VALUES
            (@ResponseActionId, @AlertId, @TenantId, @ActionDescription, @TakenAtUtc, @TakenByActorKind, @TakenByUserId, @Notes, @NowUtc);

        UPDATE [Alerts].[Alert] SET [UpdatedAtUtc] = @NowUtc WHERE [AlertId] = @AlertId AND [TenantId] = @TenantId;
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'AlertResponseAction', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ResponseActionId=', @ResponseActionId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ResponseActionId=', @ResponseActionId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
