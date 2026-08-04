-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessEvents
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessEvents_ListByZoneItems.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IAccessEventReadRepository.ListByZoneAsync
--                  (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessEvents_ListByZoneItems]
    @TenantId     UNIQUEIDENTIFIER,
    @SafetyZoneId UNIQUEIDENTIFIER,
    @FromUtc      DATETIME2(3),
    @ToUtc        DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [AccessEventId],
        [TenantId],
        [AccessPointId],
        [CredentialId],
        [SafetyZoneId],
        [EventType],
        [Direction],
        [Status],
        [OccurredAtUtc]
    FROM [PhysicalAccessControl].[AccessEvents]
    WHERE [TenantId] = @TenantId
      AND [SafetyZoneId] = @SafetyZoneId
      AND ISNULL([IsDeleted], 0) = 0
      AND [OccurredAtUtc] >= @FromUtc
      AND [OccurredAtUtc] <= @ToUtc
    ORDER BY [OccurredAtUtc] DESC;
END
GO
