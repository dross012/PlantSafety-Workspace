-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         RiskManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Risk_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Risk aggregate root row by identity. Backs
--                  IRiskRepository.GetByIdAsync. Additive-only evolution.
--  UPDATED:        2026-07-28 -- added ETag, projected from the RowVersion column:
--                  CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_Risk_GetById]
    @RiskId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        RiskId, TenantId, SiteId, Status, RiskTitle, Category, Location, HazardSource, OwnerId,
        CurrentSeverity, CurrentLikelihood, Priority, DocumentedAtUtc, ResidualSeverity, ResidualLikelihood,
        LastValidatedAtUtc, ReviewCadence, LastReviewedAtUtc, NextReviewDueUtc, ReviewRequired, ReviewCount,
        RiskVersion, CreatedAtUtc, CreatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [RiskMgmt].[Risk]
    WHERE RiskId = @RiskId AND IsDeleted = 0;
END
GO
