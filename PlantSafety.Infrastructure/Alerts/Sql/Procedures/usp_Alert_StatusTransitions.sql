-- ============================================================================
--  Module:  Alerts.Alert | Layer: Infrastructure (SQL) | Created: 2026-07-21
--  Description: Lifecycle status-transition procedures for the Alert aggregate:
--               Acknowledge, Dispatch, Escalate, Resolve, Close.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_Acknowledge]
    @AlertId         UNIQUEIDENTIFIER,
    @TenantId        UNIQUEIDENTIFIER,
    @AcknowledgedAtUtc DATETIME2(3),
    @CorrelationId   UNIQUEIDENTIFIER,
    @ActorId         UNIQUEIDENTIFIER,
    @NowUtc          DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        UPDATE [Alerts].[Alert]
        SET [Status] = N'Acknowledged',
            [AcknowledgedAtUtc] = @AcknowledgedAtUtc,
            [UpdatedAtUtc] = @NowUtc
        WHERE [AlertId] = @AlertId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Alert', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AlertId=', @AlertId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertId=', @AlertId, N'; User=', SYSTEM_USER);
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

CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_Dispatch]
    @AlertId         UNIQUEIDENTIFIER,
    @TenantId        UNIQUEIDENTIFIER,
    @DispatchedAtUtc DATETIME2(3),
    @CorrelationId   UNIQUEIDENTIFIER,
    @ActorId         UNIQUEIDENTIFIER,
    @NowUtc          DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        UPDATE [Alerts].[Alert]
        SET [Status] = N'Dispatched',
            [UpdatedAtUtc] = @NowUtc
        WHERE [AlertId] = @AlertId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Alert', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AlertId=', @AlertId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertId=', @AlertId, N'; User=', SYSTEM_USER);
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

CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_Escalate]
    @AlertId        UNIQUEIDENTIFIER,
    @TenantId       UNIQUEIDENTIFIER,
    @EscalatedAtUtc DATETIME2(3),
    @CorrelationId  UNIQUEIDENTIFIER,
    @ActorId        UNIQUEIDENTIFIER,
    @NowUtc         DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        UPDATE [Alerts].[Alert]
        SET [Status] = N'Escalated',
            [UpdatedAtUtc] = @NowUtc
        WHERE [AlertId] = @AlertId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Alert', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AlertId=', @AlertId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertId=', @AlertId, N'; User=', SYSTEM_USER);
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

CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_Resolve]
    @AlertId       UNIQUEIDENTIFIER,
    @TenantId      UNIQUEIDENTIFIER,
    @ResolvedAtUtc DATETIME2(3),
    @CorrelationId UNIQUEIDENTIFIER,
    @ActorId       UNIQUEIDENTIFIER,
    @NowUtc        DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        UPDATE [Alerts].[Alert]
        SET [Status] = N'Resolved',
            [ResolvedAtUtc] = @ResolvedAtUtc,
            [UpdatedAtUtc] = @NowUtc
        WHERE [AlertId] = @AlertId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Alert', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AlertId=', @AlertId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertId=', @AlertId, N'; User=', SYSTEM_USER);
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

-- Generic full-state overwrite used by AlertDapperRepository.UpdateAsync, which persists
-- whichever lifecycle method(s) were invoked in-memory on the aggregate since it was loaded
-- (the aggregate does not expose which transition occurred, only its resulting state).
CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_UpdateState]
    @AlertId              UNIQUEIDENTIFIER,
    @TenantId             UNIQUEIDENTIFIER,
    @Status               NVARCHAR(20),
    @SeverityCode         NVARCHAR(20),
    @AcknowledgedAtUtc    DATETIME2(3) = NULL,
    @ResolvedAtUtc        DATETIME2(3) = NULL,
    @ClosedAtUtc          DATETIME2(3) = NULL,
    @CloseOutVerification NVARCHAR(4000) = NULL,
    @CorrelationId        UNIQUEIDENTIFIER,
    @ActorId              UNIQUEIDENTIFIER,
    @NowUtc               DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        UPDATE [Alerts].[Alert]
        SET [Status] = @Status,
            [SeverityCode] = @SeverityCode,
            [AcknowledgedAtUtc] = @AcknowledgedAtUtc,
            [ResolvedAtUtc] = @ResolvedAtUtc,
            [ClosedAtUtc] = @ClosedAtUtc,
            [CloseOutVerification] = @CloseOutVerification,
            [UpdatedAtUtc] = @NowUtc
        WHERE [AlertId] = @AlertId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Alert', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AlertId=', @AlertId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertId=', @AlertId, N'; User=', SYSTEM_USER);
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

CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_Close]
    @AlertId              UNIQUEIDENTIFIER,
    @TenantId             UNIQUEIDENTIFIER,
    @ClosedAtUtc          DATETIME2(3),
    @CloseOutVerification NVARCHAR(4000),
    @CorrelationId        UNIQUEIDENTIFIER,
    @ActorId              UNIQUEIDENTIFIER,
    @NowUtc               DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        UPDATE [Alerts].[Alert]
        SET [Status] = N'Closed',
            [ClosedAtUtc] = @ClosedAtUtc,
            [CloseOutVerification] = @CloseOutVerification,
            [UpdatedAtUtc] = @NowUtc
        WHERE [AlertId] = @AlertId AND [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0;
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Alert', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AlertId=', @AlertId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertId=', @AlertId, N'; User=', SYSTEM_USER);
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
