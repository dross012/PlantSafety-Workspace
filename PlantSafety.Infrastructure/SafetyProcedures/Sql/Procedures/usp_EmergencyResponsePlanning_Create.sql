-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.EmergencyResponsePlanning
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencyResponsePlanning_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [SafetyProcedures].[EmergencyResponsePlanning], backing
--                  IEmergencyResponsePlanningRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_EmergencyResponsePlanning_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
        INSERT INTO [SafetyProcedures].[EmergencyResponsePlanning]
            ([EmergencyResponsePlanningId], [TenantId], [SiteId], [Status], [PlanTitle], [EmergencyType], [EvacuationRoute], [ApprovedByUserId], [DrillConductedAtUtc], [NextDrillDueAtUtc], [ParticipantCount], [ReadinessScore], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@EmergencyResponsePlanningId, @TenantId, @SiteId, @Status, @PlanTitle, @EmergencyType, @EvacuationRoute, @ApprovedByUserId, @DrillConductedAtUtc, @NextDrillDueAtUtc, @ParticipantCount, @ReadinessScore, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
