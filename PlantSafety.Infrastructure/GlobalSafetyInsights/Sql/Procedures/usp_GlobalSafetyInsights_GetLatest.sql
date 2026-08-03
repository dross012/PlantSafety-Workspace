-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         GlobalSafetyInsights
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_GlobalSafetyInsights_GetLatest.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
--  SchemaVersion:  2026.1
--  Description:    Retrieves the single most recently computed snapshot for TenantId. Backs
--                  IGlobalSafetyInsightsRepository.GetLatestAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [GlobalSafetyInsights].[usp_GlobalSafetyInsights_GetLatest]
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT TOP (1) *
    FROM [GlobalSafetyInsights].[Snapshot]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
    ORDER BY ComputedAtUtc DESC;
END
GO
