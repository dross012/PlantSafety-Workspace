-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkforceSafety
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_WorkforceSafety_List.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[WorkforceSafetyReadModel],
--                  backing IWorkforceSafetyRepository.GetWorkforceSafetyAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_WorkforceSafety_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [WorkforceSafetyRecordId],
        [TenantId],
        [SiteId],
        [MetricId],
        [MetricName],
        [MetricValue],
        [Status],
        [RecordedAtUtc],
        [RecordedBy]
FROM [Operational].[WorkforceSafetyReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [RecordedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [RecordedAtUtc] DESC;
GO