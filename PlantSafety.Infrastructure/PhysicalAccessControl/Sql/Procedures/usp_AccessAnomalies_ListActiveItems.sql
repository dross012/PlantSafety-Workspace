-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessAnomalies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessAnomalies_ListActiveItems.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IAccessAnomalyReadRepository.ListActiveAsync
--                  (Application read-model, backs the Anomaly Feed dashboard).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessAnomalies_ListActiveItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [AccessAnomalyId],
        [TenantId],
        [AccessPointId],
        [CredentialId],
        [SafetyZoneId],
        [AnomalyType],
        [SeverityScore],
        [Status],
        [CreatedAtUtc]
    FROM [PhysicalAccessControl].[AccessAnomalies]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [Status] <> 'Resolved'
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [SeverityScore] DESC, [CreatedAtUtc] DESC;
END
GO
