-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PlanningProjects.PlanningProjects
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_PlanningProjects_GetProjects.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IPlanningProjectsRepository.GetProjectsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PlanningProjects].[usp_PlanningProjects_GetProjects]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ProjectId,
        TenantId,
        SiteId,
        Name,
        Status,
        TargetCompletionUtc,
        OwnerId
    FROM [PlanningProjects].[PlanningProjects]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND TargetCompletionUtc >= @FromUtc
      AND TargetCompletionUtc <= @ToUtc
    ORDER BY TargetCompletionUtc DESC;
END
GO
