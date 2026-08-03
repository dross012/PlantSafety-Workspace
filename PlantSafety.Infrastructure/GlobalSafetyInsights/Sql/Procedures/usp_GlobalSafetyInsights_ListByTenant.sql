-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         GlobalSafetyInsights
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_GlobalSafetyInsights_ListByTenant.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
--  SchemaVersion:  2026.1
--  Description:    Retrieves every snapshot for TenantId, most recent first. Backs
--                  IGlobalSafetyInsightsRepository.ListAsync (specification is evaluated
--                  in-memory over this candidate set) and
--                  IGlobalSafetyInsightsReadRepository.GetHistoryAsync (date-range filtered here).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [GlobalSafetyInsights].[usp_GlobalSafetyInsights_ListByTenant]
    @TenantId UNIQUEIDENTIFIER,
    @FromUtc  DATETIME2(3) = NULL,
    @ToUtc    DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT *
    FROM [GlobalSafetyInsights].[Snapshot]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@FromUtc IS NULL OR ComputedAtUtc >= @FromUtc)
      AND (@ToUtc IS NULL OR ComputedAtUtc <= @ToUtc)
    ORDER BY ComputedAtUtc DESC;
END
GO
