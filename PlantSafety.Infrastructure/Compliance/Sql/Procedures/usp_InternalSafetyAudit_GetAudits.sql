-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Auditing.InternalSafetyAudit
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_InternalSafetyAudit_GetAudits.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IInternalSafetyAuditRepository.GetAuditsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_InternalSafetyAudit_GetAudits]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        AuditingId,
        TenantId,
        SiteId,
        Scope,
        Status,
        ConductedAtUtc,
        ConductedBy
    FROM [Compliance].[InternalSafetyAudit]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ConductedAtUtc >= @FromUtc
      AND ConductedAtUtc <= @ToUtc
    ORDER BY ConductedAtUtc DESC;
END
GO
