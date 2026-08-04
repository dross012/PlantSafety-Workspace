-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.RestrictedZonePolicies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RestrictedZonePolicies_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves all non-deleted RestrictedZonePolicy rows for in-memory
--                  specification filtering by ListAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_RestrictedZonePolicies_GetAll]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [RestrictedZonePolicyId], [TenantId], [SiteId], [Status], [SafetyZoneId],
        [PolicyName], [AllowedAccessLevel], [EnforceAfterHoursLockout], [CreatedAtUtc], [CreatedBy]
    FROM [PhysicalAccessControl].[RestrictedZonePolicies]
    WHERE ISNULL([IsDeleted], 0) = 0;
END
GO
