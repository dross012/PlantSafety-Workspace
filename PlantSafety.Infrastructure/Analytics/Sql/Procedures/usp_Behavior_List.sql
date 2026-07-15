-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture.Behavior
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Behavior_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[Behavior],
--                  backing IBehaviorRepository.GetBehaviorObservationsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Behavior_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ObservationId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [BehaviorCategory],
        [ObservationType],
        [Status],
        [ObservedAtUtc],
        [ObservedBy]
FROM [Analytics].[Behavior]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ObservedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ObservedAtUtc] DESC;
GO
