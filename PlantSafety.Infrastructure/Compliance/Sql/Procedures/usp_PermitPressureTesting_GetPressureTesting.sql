-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.PressureTesting
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_PermitPressureTesting_GetPressureTesting.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IPressureTestingRepository.GetPressureTestingAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitPressureTesting_GetPressureTesting]
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
        TestArticle,
        TestPressurePsi,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitPressureTesting]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
GO
