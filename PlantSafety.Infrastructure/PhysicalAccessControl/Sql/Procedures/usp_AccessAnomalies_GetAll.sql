-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessAnomalies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessAnomalies_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves all non-deleted AccessAnomaly rows for in-memory specification
--                  filtering by ListAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessAnomalies_GetAll]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [AccessAnomalyId], [TenantId], [SiteId], [Status], [AccessEventId], [AccessPointId],
        [CredentialId], [SafetyZoneId], [AnomalyType], [SeverityScore], [AcknowledgedByUserId],
        [Resolution], [CreatedAtUtc], [CreatedBy]
    FROM [PhysicalAccessControl].[AccessAnomalies]
    WHERE ISNULL([IsDeleted], 0) = 0;
END
GO
