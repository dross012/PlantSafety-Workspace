-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         RiskManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Risk_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted Risk aggregate root rows. Backs
--                  IRiskRepository.ListAsync, whose RiskSpecification predicate is applied
--                  in-memory over the candidate set returned here. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_Risk_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        RiskId, TenantId, SiteId, Status, RiskTitle, Category, Location, HazardSource, OwnerId,
        CurrentSeverity, CurrentLikelihood, Priority, DocumentedAtUtc, ResidualSeverity, ResidualLikelihood,
        LastValidatedAtUtc, ReviewCadence, LastReviewedAtUtc, NextReviewDueUtc, ReviewRequired, ReviewCount,
        RiskVersion, CreatedAtUtc, CreatedBy
    FROM [RiskMgmt].[Risk]
    WHERE IsDeleted = 0;
END
GO
