-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.EmergencyPreparedness
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencyPreparedness_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [EmployeeSafety].[EmergencyPreparedness], backing
--                  IEmergencyPreparednessRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_EmergencyPreparedness_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @EmergencyPreparednessId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @PlanTitle NVARCHAR(400),
    @EmergencyType NVARCHAR(100),
    @EvacuationRoute NVARCHAR(400),
    @CoordinatedByUserId UNIQUEIDENTIFIER,
    @DrillConductedAtUtc DATETIME2(3) = NULL,
    @NextDrillDueAtUtc DATETIME2(3) = NULL,
    @ParticipantCount INT,
    @ReadinessScore DECIMAL(9,2) = NULL,
    @ReadinessThreshold DECIMAL(9,2),
    @ReadinessGapNotes NVARCHAR(2000) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [EmployeeSafety].[EmergencyPreparedness]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [PlanTitle] = @PlanTitle,
        [EmergencyType] = @EmergencyType,
        [EvacuationRoute] = @EvacuationRoute,
        [CoordinatedByUserId] = @CoordinatedByUserId,
        [DrillConductedAtUtc] = @DrillConductedAtUtc,
        [NextDrillDueAtUtc] = @NextDrillDueAtUtc,
        [ParticipantCount] = @ParticipantCount,
        [ReadinessScore] = @ReadinessScore,
        [ReadinessThreshold] = @ReadinessThreshold,
        [ReadinessGapNotes] = @ReadinessGapNotes,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [EmergencyPreparednessId] = @EmergencyPreparednessId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
