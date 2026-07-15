-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.CrisisManagement
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_CrisisManagement_GetCrisisEvents.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ICrisisManagementRepository.GetCrisisEventsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_CrisisManagement_GetCrisisEvents]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CrisisId,
        TenantId,
        SiteId,
        Title,
        Severity,
        Status,
        DeclaredAtUtc,
        DeclaredBy
    FROM [SafetyEvents].[CrisisManagement]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND DeclaredAtUtc >= @FromUtc
      AND DeclaredAtUtc <= @ToUtc
    ORDER BY DeclaredAtUtc DESC;
END
GO
