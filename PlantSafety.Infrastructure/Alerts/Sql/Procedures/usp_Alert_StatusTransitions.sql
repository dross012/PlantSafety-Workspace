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
    END TRY
    BEGIN CATCH
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
    END TRY
    BEGIN CATCH
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
    END TRY
    BEGIN CATCH
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
    END TRY
    BEGIN CATCH
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
    END TRY
    BEGIN CATCH
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
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
