-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.FatigueManagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_FatigueManagement_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[FatigueManagementReadModel],
--                  backing IFatigueManagementRepository.GetFatigueManagementAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_FatigueManagement_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [FatigueRecordId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [FatigueRiskLevel],
        [Status],
        [RecordedAtUtc],
        [RecordedBy]
FROM [Operational].[FatigueManagementReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [RecordedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [RecordedAtUtc] DESC;
GO
