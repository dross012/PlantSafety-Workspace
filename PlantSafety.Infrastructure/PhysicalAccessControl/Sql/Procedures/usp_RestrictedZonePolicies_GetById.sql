-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.RestrictedZonePolicies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RestrictedZonePolicies_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single RestrictedZonePolicy row by identity for aggregate
--                  rehydration.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_RestrictedZonePolicies_GetById]
    @RestrictedZonePolicyId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [RestrictedZonePolicyId], [TenantId], [SiteId], [Status], [SafetyZoneId],
        [PolicyName], [AllowedAccessLevel], [EnforceAfterHoursLockout], [CreatedAtUtc], [CreatedBy]
    FROM [PhysicalAccessControl].[RestrictedZonePolicies]
    WHERE [RestrictedZonePolicyId] = @RestrictedZonePolicyId
      AND ISNULL([IsDeleted], 0) = 0;
END
GO
