-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.SafetyValvesRelays
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyValvesRelays_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [MachineSafety].[SafetyValvesRelays], backing
--                  ISafetyValvesRelaysRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_SafetyValvesRelays_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @SafetyValvesRelaysId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ComponentTitle NVARCHAR(400),
    @ComponentType NVARCHAR(100),
    @EquipmentLocation NVARCHAR(400),
    @InspectedByUserId UNIQUEIDENTIFIER,
    @InspectedAtUtc DATETIME2(3) = NULL,
    @NextInspectionDueAtUtc DATETIME2(3) = NULL,
    @FaultCount INT,
    @ReliabilityScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [MachineSafety].[SafetyValvesRelays]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [ComponentTitle] = @ComponentTitle,
        [ComponentType] = @ComponentType,
        [EquipmentLocation] = @EquipmentLocation,
        [InspectedByUserId] = @InspectedByUserId,
        [InspectedAtUtc] = @InspectedAtUtc,
        [NextInspectionDueAtUtc] = @NextInspectionDueAtUtc,
        [FaultCount] = @FaultCount,
        [ReliabilityScore] = @ReliabilityScore,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [SafetyValvesRelaysId] = @SafetyValvesRelaysId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
