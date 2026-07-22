-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PPE
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PPEZoneMapping_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single PPEZoneMapping row by identity. Backs IPPEZoneMappingRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PPE].[usp_PPEZoneMapping_GetById]
    @PPEZoneMappingId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT PPEZoneMappingId, TenantId, SiteId, Status, ZoneCode, RequiredPpeCategories, AccessControlIntegrated, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PPE].[PPEZoneMapping]
    WHERE PPEZoneMappingId = @PPEZoneMappingId AND IsDeleted = 0;
END
GO
