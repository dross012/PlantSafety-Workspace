-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Auditing.AuditFindings
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_AuditFindings_GetFindings.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IAuditFindingsRepository.GetFindingsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_AuditFindings_GetFindings]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        FindingId,
        AuditingId,
        TenantId,
        SiteId,
        Title,
        Severity,
        Status,
        IdentifiedAtUtc,
        IdentifiedBy
    FROM [Compliance].[AuditFindings]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IdentifiedAtUtc >= @FromUtc
      AND IdentifiedAtUtc <= @ToUtc
    ORDER BY IdentifiedAtUtc DESC;
END
GO
