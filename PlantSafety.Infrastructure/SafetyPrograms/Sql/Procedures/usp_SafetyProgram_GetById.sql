-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPrograms.SafetyProgram
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyProgram_GetById.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieval-by-id procedure for [SafetyPrograms].[SafetyProgram], backing
--                  ISafetyProgramRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyPrograms].[usp_SafetyProgram_GetById]
    @SafetyProgramId UNIQUEIDENTIFIER,
    @TenantId        UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [SafetyProgramId], [TenantId], [SiteId], [Status], [Name], [Description], [Category],
    [StartDate], [EndDate], [OwnerId], [ParticipationPercent], [EnrolledCount], [Goals],
    [CreatedAtUtc], [CreatedBy]
FROM [SafetyPrograms].[SafetyProgram]
WHERE [SafetyProgramId] = @SafetyProgramId
  AND [TenantId] = @TenantId
  AND [IsDeleted] = 0;
GO
