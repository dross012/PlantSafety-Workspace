-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Certification.Contractor
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_CertificationContractor_GetContractor.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IContractorRepository.GetContractorAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_CertificationContractor_GetContractor]
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
        ContractorName,
        CertificationType,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        VerifiedBy
    FROM [Compliance].[CertificationContractor]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
GO
