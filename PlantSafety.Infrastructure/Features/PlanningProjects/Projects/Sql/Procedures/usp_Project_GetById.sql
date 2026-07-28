-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.PlanningProjects.Projects
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Project_GetById.sql
--  Created:        2026-07-28
--  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IProjectRepository.GetByIdAsync.
--                  Projects ETag from RowVersion: CONVERT(NVARCHAR(34), CAST(RowVersion AS
--                  BINARY(8)), 1), matching usp_Roi_GetByFiscalYear / usp_Risk_GetById precedent.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PlanningProjects].[usp_Project_GetById]
    @TenantId  UNIQUEIDENTIFIER,
    @ProjectId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ProjectId,
        TenantId,
        SiteId,
        Name,
        Description,
        Status,
        Priority,
        ProjectOwnerId,
        ProjectManagerId,
        PlannedStartUtc,
        PlannedEndUtc,
        ActualStartUtc,
        ActualEndUtc,
        PlannedBudget,
        OpenMilestoneCount,
        LastModifiedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [PlanningProjects].[Project]
    WHERE ProjectId = @ProjectId
      AND TenantId = @TenantId
      AND IsDeleted = 0;
END
GO
