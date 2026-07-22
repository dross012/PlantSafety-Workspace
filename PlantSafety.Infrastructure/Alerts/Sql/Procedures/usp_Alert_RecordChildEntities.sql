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
    END TRY
    BEGIN CATCH
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
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
