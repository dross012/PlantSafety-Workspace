-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.HazardCommunicationTraining
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HazardCommunicationTraining_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted HazardCommunicationTraining rows, backing
--                  IHazardCommunicationTrainingRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_HazardCommunicationTraining_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [HazardCommunicationTrainingId], [TenantId], [SiteId], [Status], [ProgramTitle], [HazardClass], [SdsReference], [TrainedByUserId], [TrainingDeliveredAtUtc], [EmployeesTrainedCount], [ComprehensionScore], [PassingScoreThreshold], [NextTrainingDueAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[HazardCommunicationTraining]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
