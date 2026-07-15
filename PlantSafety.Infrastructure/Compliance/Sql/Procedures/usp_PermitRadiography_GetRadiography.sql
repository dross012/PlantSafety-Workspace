-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.Radiography
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_PermitRadiography_GetRadiography.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IRadiographyRepository.GetRadiographyAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitRadiography_GetRadiography]
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
        SourceIsotope,
        ExclusionZoneMeters,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitRadiography]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
GO
