-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Training
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Training_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[TrainingReadModel],
--                  backing ITrainingRepository.GetTrainingAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_Training_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [TrainingRecordId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [CourseName],
        [Status],
        [CompletedAtUtc],
        [CertifiedBy]
FROM [Operational].[TrainingReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CompletedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CompletedAtUtc] DESC;
GO
