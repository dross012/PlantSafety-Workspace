-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Certification.ISO45001
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_CertificationISO45001_GetISO45001.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IISO45001Repository.GetISO45001Async.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_CertificationISO45001_GetISO45001]
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
        CertifyingBody,
        Scope,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        VerifiedBy
    FROM [Compliance].[CertificationISO45001]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
GO
