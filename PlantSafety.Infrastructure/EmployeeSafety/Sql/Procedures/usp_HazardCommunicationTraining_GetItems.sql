-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.HazardCommunicationTraining
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HazardCommunicationTraining_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IHazardCommunicationTrainingRepository.GetHazardCommunicationTrainingItemsAsync (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_HazardCommunicationTraining_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [HazardCommunicationTrainingId],
        [TenantId],
        [SiteId],
        [ProgramTitle],
        [HazardClass] AS [HazardClass],
        [SdsReference],
        [Status] AS [TrainingStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [EmployeeSafety].[HazardCommunicationTraining]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO
