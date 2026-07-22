-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PPE
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PPEZoneMapping_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PPEZoneMapping rows. Backs IPPEZoneMappingRepository.ListAsync
--                  (specification is evaluated in-memory over this candidate set,
--                  since the interface carries no tenant/site scoping parameter).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PPE].[usp_PPEZoneMapping_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT PPEZoneMappingId, TenantId, SiteId, Status, ZoneCode, RequiredPpeCategories, AccessControlIntegrated, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PPE].[PPEZoneMapping]
    WHERE IsDeleted = 0;
END
GO
