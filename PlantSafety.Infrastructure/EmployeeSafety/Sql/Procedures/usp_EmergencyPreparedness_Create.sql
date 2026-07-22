-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.EmergencyPreparedness
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencyPreparedness_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [EmployeeSafety].[EmergencyPreparedness], backing
--                  IEmergencyPreparednessRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_EmergencyPreparedness_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
        INSERT INTO [EmployeeSafety].[EmergencyPreparedness]
            ([EmergencyPreparednessId], [TenantId], [SiteId], [Status], [PlanTitle], [EmergencyType], [EvacuationRoute], [CoordinatedByUserId], [DrillConductedAtUtc], [NextDrillDueAtUtc], [ParticipantCount], [ReadinessScore], [ReadinessThreshold], [ReadinessGapNotes], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@EmergencyPreparednessId, @TenantId, @SiteId, @Status, @PlanTitle, @EmergencyType, @EvacuationRoute, @CoordinatedByUserId, @DrillConductedAtUtc, @NextDrillDueAtUtc, @ParticipantCount, @ReadinessScore, @ReadinessThreshold, @ReadinessGapNotes, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
