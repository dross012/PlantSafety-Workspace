-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         GlobalSafetyInsights
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_GlobalSafetyInsights_GetById.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single GlobalSafetyInsights snapshot by id, scoped to TenantId
--                  (zero cross-tenant leakage). Backs IGlobalSafetyInsightsRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [GlobalSafetyInsights].[usp_GlobalSafetyInsights_GetById]
    @GlobalSafetyInsightsId UNIQUEIDENTIFIER,
    @TenantId               UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT *
    FROM [GlobalSafetyInsights].[Snapshot]
    WHERE GlobalSafetyInsightsId = @GlobalSafetyInsightsId
      AND TenantId = @TenantId
      AND IsDeleted = 0;
END
GO
