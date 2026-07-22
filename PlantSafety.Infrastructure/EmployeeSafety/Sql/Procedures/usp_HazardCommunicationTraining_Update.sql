-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.HazardCommunicationTraining
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HazardCommunicationTraining_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [EmployeeSafety].[HazardCommunicationTraining], backing
--                  IHazardCommunicationTrainingRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_HazardCommunicationTraining_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @HazardCommunicationTrainingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ProgramTitle NVARCHAR(400),
    @HazardClass NVARCHAR(100),
    @SdsReference NVARCHAR(400),
    @TrainedByUserId UNIQUEIDENTIFIER,
    @TrainingDeliveredAtUtc DATETIME2(3) = NULL,
    @EmployeesTrainedCount INT,
    @ComprehensionScore DECIMAL(9,2) = NULL,
    @PassingScoreThreshold DECIMAL(9,2),
    @NextTrainingDueAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [EmployeeSafety].[HazardCommunicationTraining]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [ProgramTitle] = @ProgramTitle,
        [HazardClass] = @HazardClass,
        [SdsReference] = @SdsReference,
        [TrainedByUserId] = @TrainedByUserId,
        [TrainingDeliveredAtUtc] = @TrainingDeliveredAtUtc,
        [EmployeesTrainedCount] = @EmployeesTrainedCount,
        [ComprehensionScore] = @ComprehensionScore,
        [PassingScoreThreshold] = @PassingScoreThreshold,
        [NextTrainingDueAtUtc] = @NextTrainingDueAtUtc,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [HazardCommunicationTrainingId] = @HazardCommunicationTrainingId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
