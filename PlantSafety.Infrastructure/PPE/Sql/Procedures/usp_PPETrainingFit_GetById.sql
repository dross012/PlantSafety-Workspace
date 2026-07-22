-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PPE
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PPETrainingFit_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single PPETrainingFit row by identity. Backs IPPETrainingFitRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PPE].[usp_PPETrainingFit_GetById]
    @PPETrainingFitId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT PPETrainingFitId, TenantId, SiteId, Status, WorkerId, TrainingType, FitTestExpiresAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PPE].[PPETrainingFit]
    WHERE PPETrainingFitId = @PPETrainingFitId AND IsDeleted = 0;
END
GO
