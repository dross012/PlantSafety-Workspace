-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessEvents
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessEvents_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves all non-deleted AccessEvent rows for in-memory specification
--                  filtering by ListAsync (e.g. scheduled access-log reconciliation).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessEvents_GetAll]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [AccessEventId], [TenantId], [SiteId], [Status], [AccessPointId], [CredentialId],
        [SafetyZoneId], [EventType], [Direction], [OccurredAtUtc], [FlaggedAnomalyType],
        [CreatedAtUtc], [CreatedBy]
    FROM [PhysicalAccessControl].[AccessEvents]
    WHERE ISNULL([IsDeleted], 0) = 0;
END
GO
