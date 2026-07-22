-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.HazardCommunicationTraining
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HazardCommunicationTraining_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single HazardCommunicationTraining row by identifier, backing
--                  IHazardCommunicationTrainingRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_HazardCommunicationTraining_GetById]
(
    @HazardCommunicationTrainingId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [HazardCommunicationTrainingId], [TenantId], [SiteId], [Status], [ProgramTitle], [HazardClass], [SdsReference], [TrainedByUserId], [TrainingDeliveredAtUtc], [EmployeesTrainedCount], [ComprehensionScore], [PassingScoreThreshold], [NextTrainingDueAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[HazardCommunicationTraining]
    WHERE [HazardCommunicationTrainingId] = @HazardCommunicationTrainingId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
