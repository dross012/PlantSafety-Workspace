-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPrograms.SafetyProgram
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyProgram_Count.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Count procedure for [SafetyPrograms].[SafetyProgram], backing
--                  ISafetyProgramRepository.CountAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyPrograms].[usp_SafetyProgram_Count]
    @TenantId     UNIQUEIDENTIFIER,
    @SiteId       UNIQUEIDENTIFIER = NULL,
    @StatusFilter NVARCHAR(20) = NULL
AS
SET NOCOUNT ON;
SELECT COUNT(*)
FROM [SafetyPrograms].[SafetyProgram]
WHERE [TenantId] = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND (@StatusFilter IS NULL OR [Status] = @StatusFilter)
  AND [IsDeleted] = 0;
GO
