-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.NearMisses
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_NearMisses_GetNearMisses.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  INearMissesRepository.GetNearMissesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_NearMisses_GetNearMisses]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        NearMissId,
        TenantId,
        SiteId,
        Category,
        PotentialSeverity,
        Status,
        ReportedAtUtc,
        ReportedBy
    FROM [SafetyEvents].[NearMisses]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ReportedAtUtc >= @FromUtc
      AND ReportedAtUtc <= @ToUtc
    ORDER BY ReportedAtUtc DESC;
END
GO
