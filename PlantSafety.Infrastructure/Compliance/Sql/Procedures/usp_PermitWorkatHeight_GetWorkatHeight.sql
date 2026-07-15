-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.WorkatHeight
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_PermitWorkatHeight_GetWorkatHeight.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IWorkatHeightRepository.GetWorkatHeightAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitWorkatHeight_GetWorkatHeight]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        WorkLocation,
        FallProtectionType,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitWorkatHeight]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
GO
