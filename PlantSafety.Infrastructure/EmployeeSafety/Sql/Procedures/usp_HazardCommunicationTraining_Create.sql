-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.HazardCommunicationTraining
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HazardCommunicationTraining_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [EmployeeSafety].[HazardCommunicationTraining], backing
--                  IHazardCommunicationTrainingRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_HazardCommunicationTraining_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
        INSERT INTO [EmployeeSafety].[HazardCommunicationTraining]
            ([HazardCommunicationTrainingId], [TenantId], [SiteId], [Status], [ProgramTitle], [HazardClass], [SdsReference], [TrainedByUserId], [TrainingDeliveredAtUtc], [EmployeesTrainedCount], [ComprehensionScore], [PassingScoreThreshold], [NextTrainingDueAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@HazardCommunicationTrainingId, @TenantId, @SiteId, @Status, @ProgramTitle, @HazardClass, @SdsReference, @TrainedByUserId, @TrainingDeliveredAtUtc, @EmployeesTrainedCount, @ComprehensionScore, @PassingScoreThreshold, @NextTrainingDueAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
