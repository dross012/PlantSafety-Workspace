-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPrograms.SafetyProgram
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyProgram_List.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Paginated, optionally status-filtered list procedure for
--                  [SafetyPrograms].[SafetyProgram], backing ISafetyProgramRepository.ListAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyPrograms].[usp_SafetyProgram_List]
    @TenantId     UNIQUEIDENTIFIER,
    @SiteId       UNIQUEIDENTIFIER = NULL,
    @StatusFilter NVARCHAR(20) = NULL,
    @PageNumber   INT = 1,
    @PageSize     INT = 25
AS
SET NOCOUNT ON;
SELECT
    [SafetyProgramId], [TenantId], [SiteId], [Status], [Name], [Description], [Category],
    [StartDate], [EndDate], [OwnerId], [ParticipationPercent], [EnrolledCount], [Goals],
    [CreatedAtUtc], [CreatedBy]
FROM [SafetyPrograms].[SafetyProgram]
WHERE [TenantId] = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND (@StatusFilter IS NULL OR [Status] = @StatusFilter)
  AND [IsDeleted] = 0
ORDER BY [CreatedAtUtc] DESC, [SafetyProgramId]
OFFSET (@PageNumber - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY;
GO
