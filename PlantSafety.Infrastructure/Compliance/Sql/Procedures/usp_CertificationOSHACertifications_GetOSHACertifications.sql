-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Certification.OSHACertifications
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_CertificationOSHACertifications_GetOSHACertifications.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IOSHACertificationsRepository.GetOSHACertificationsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_CertificationOSHACertifications_GetOSHACertifications]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CertificationId,
        TenantId,
        SiteId,
        HolderName,
        OshaStandard,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        VerifiedBy
    FROM [Compliance].[CertificationOSHACertifications]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
GO
