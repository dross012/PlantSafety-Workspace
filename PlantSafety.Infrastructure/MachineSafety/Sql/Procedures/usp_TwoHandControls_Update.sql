-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.TwoHandControls
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_TwoHandControls_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [MachineSafety].[TwoHandControls], backing
--                  ITwoHandControlsRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_TwoHandControls_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @TwoHandControlsId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ControlTitle NVARCHAR(400),
    @EquipmentType NVARCHAR(100),
    @EquipmentLocation NVARCHAR(400),
    @TestedByUserId UNIQUEIDENTIFIER,
    @TestedAtUtc DATETIME2(3) = NULL,
    @NextTestDueAtUtc DATETIME2(3) = NULL,
    @BypassAttemptCount INT,
    @SynchronizationScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [MachineSafety].[TwoHandControls]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [ControlTitle] = @ControlTitle,
        [EquipmentType] = @EquipmentType,
        [EquipmentLocation] = @EquipmentLocation,
        [TestedByUserId] = @TestedByUserId,
        [TestedAtUtc] = @TestedAtUtc,
        [NextTestDueAtUtc] = @NextTestDueAtUtc,
        [BypassAttemptCount] = @BypassAttemptCount,
        [SynchronizationScore] = @SynchronizationScore,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [TwoHandControlsId] = @TwoHandControlsId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
