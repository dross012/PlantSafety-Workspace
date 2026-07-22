-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.AITraining
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_AITraining_GetTrainingJobs.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IAITrainingRepository.GetTrainingJobsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Intelligence].[usp_AITraining_GetTrainingJobs]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        TrainingJobId,
        TenantId,
        SiteId,
        ModelName,
        ModelVersion,
        DatasetReference,
        Status,
        StartedAtUtc,
        InitiatedBy
    FROM [Intelligence].[AITraining]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND StartedAtUtc >= @FromUtc
      AND StartedAtUtc <= @ToUtc
    ORDER BY StartedAtUtc DESC;
END
GO
