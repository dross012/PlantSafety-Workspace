-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Auditing.CAPATriggers
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_CAPATriggers_GetTriggers.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ICAPATriggersRepository.GetTriggersAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_CAPATriggers_GetTriggers]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        TriggerId,
        AuditingId,
        SourceFindingId,
        TenantId,
        SiteId,
        Reason,
        Status,
        TriggeredAtUtc,
        TriggeredBy
    FROM [Compliance].[CAPATriggers]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND TriggeredAtUtc >= @FromUtc
      AND TriggeredAtUtc <= @ToUtc
    ORDER BY TriggeredAtUtc DESC;
END
GO
