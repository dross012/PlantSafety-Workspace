-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessAnomalies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessAnomalies_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single AccessAnomaly row by identity for aggregate rehydration.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessAnomalies_GetById]
    @AccessAnomalyId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [AccessAnomalyId], [TenantId], [SiteId], [Status], [AccessEventId], [AccessPointId],
        [CredentialId], [SafetyZoneId], [AnomalyType], [SeverityScore], [AcknowledgedByUserId],
        [Resolution], [CreatedAtUtc], [CreatedBy]
    FROM [PhysicalAccessControl].[AccessAnomalies]
    WHERE [AccessAnomalyId] = @AccessAnomalyId
      AND ISNULL([IsDeleted], 0) = 0;
END
GO
