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
--  UPDATED:        2026-07-28 -- added ETag, projected from the RowVersion column the same way as
--                  usp_Risk_GetById.sql: CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1).
--  UPDATED:        2026-07-29 -- added @TenantId (Phase 4 cross-tenant access remediation): this
--                  proc previously returned every tenant's active Risks with no filtering at all.
--                  ListAsync/RiskSpecification currently has no callers, but leaving an
--                  unscoped "return everything" proc in place is a live landmine for whenever it
--                  is wired up -- fixed now rather than left as a dormant cross-tenant leak.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_Risk_GetAllActive]
    @TenantId UNIQUEIDENTIFIER
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
    WHERE TenantId = @TenantId AND IsDeleted = 0;
END
GO
