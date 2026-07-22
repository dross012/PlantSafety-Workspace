-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.EmergencyResponsePlanning
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencyResponsePlanning_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [SafetyProcedures].[EmergencyResponsePlanning], backing
--                  IEmergencyResponsePlanningRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_EmergencyResponsePlanning_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @EmergencyResponsePlanningId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @PlanTitle NVARCHAR(400),
    @EmergencyType NVARCHAR(100),
    @EvacuationRoute NVARCHAR(400),
    @ApprovedByUserId UNIQUEIDENTIFIER,
    @DrillConductedAtUtc DATETIME2(3) = NULL,
    @NextDrillDueAtUtc DATETIME2(3) = NULL,
    @ParticipantCount INT,
    @ReadinessScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [SafetyProcedures].[EmergencyResponsePlanning]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [PlanTitle] = @PlanTitle,
        [EmergencyType] = @EmergencyType,
        [EvacuationRoute] = @EvacuationRoute,
        [ApprovedByUserId] = @ApprovedByUserId,
        [DrillConductedAtUtc] = @DrillConductedAtUtc,
        [NextDrillDueAtUtc] = @NextDrillDueAtUtc,
        [ParticipantCount] = @ParticipantCount,
        [ReadinessScore] = @ReadinessScore,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [EmergencyResponsePlanningId] = @EmergencyResponsePlanningId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
