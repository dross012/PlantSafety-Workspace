-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HealthChecks
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_HealthChecks_GetChecks.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IHealthChecksRepository.GetChecksAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HealthChecks].[usp_HealthChecks_GetChecks]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CheckId,
        TenantId,
        SiteId,
        CheckName,
        Status,
        CheckedAtUtc,
        CheckedBy
    FROM [HealthChecks].[HealthChecks]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND CheckedAtUtc >= @FromUtc
      AND CheckedAtUtc <= @ToUtc
    ORDER BY CheckedAtUtc DESC;
END
GO
