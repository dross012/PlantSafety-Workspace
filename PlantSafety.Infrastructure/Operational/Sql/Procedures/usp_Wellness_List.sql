-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Wellness
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Wellness_List.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[WellnessReadModel],
--                  backing IWellnessRepository.GetWellnessAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_Wellness_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [WellnessCheckInId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [EmployeeName],
        [CheckInType],
        [Status],
        [CheckedInAtUtc],
        [CheckedInBy]
FROM [Operational].[WellnessReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CheckedInAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CheckedInAtUtc] DESC;
GO