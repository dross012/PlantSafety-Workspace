-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Certification.ISO14001
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_CertificationISO14001_GetISO14001.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IISO14001Repository.GetISO14001Async.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_CertificationISO14001_GetISO14001]
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
    FROM [Compliance].[CertificationISO14001]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
GO
