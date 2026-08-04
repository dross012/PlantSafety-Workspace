-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessEvents
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessEvents_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single AccessEvent row by identity for aggregate rehydration.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessEvents_GetById]
    @AccessEventId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [AccessEventId], [TenantId], [SiteId], [Status], [AccessPointId], [CredentialId],
        [SafetyZoneId], [EventType], [Direction], [OccurredAtUtc], [FlaggedAnomalyType],
        [CreatedAtUtc], [CreatedBy]
    FROM [PhysicalAccessControl].[AccessEvents]
    WHERE [AccessEventId] = @AccessEventId
      AND ISNULL([IsDeleted], 0) = 0;
END
GO
