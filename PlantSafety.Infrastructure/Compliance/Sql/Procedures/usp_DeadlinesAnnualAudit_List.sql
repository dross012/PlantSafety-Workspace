-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.DeadlinesAnnualAudit
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DeadlinesAnnualAudit_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[DeadlinesAnnualAudit],
--                  backing IAnnualAuditRepository.GetDeadlinesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_DeadlinesAnnualAudit_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [DeadlineId],
        [TenantId],
        [SiteId],
        [AuditScope],
        [DueAtUtc],
        [Status],
        [ResponsibleParty]
FROM [Compliance].[DeadlinesAnnualAudit]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [DueAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [DueAtUtc] DESC;
GO