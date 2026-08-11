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
        -- Canonical audit trail (Handbook §2.11): the write is recorded after it commits, in its
        -- own TRY/CATCH so an audit failure can never fail the operation it is describing.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'EmergencyResponsePlanning', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'EmergencyResponsePlanningId=', @EmergencyResponsePlanningId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EmergencyResponsePlanningId=', @EmergencyResponsePlanningId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

    THROW;
END CATCH;
GO
