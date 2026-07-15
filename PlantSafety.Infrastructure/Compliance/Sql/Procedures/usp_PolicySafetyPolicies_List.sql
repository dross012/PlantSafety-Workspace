-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicySafetyPolicies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PolicySafetyPolicies_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[PolicySafetyPolicies],
--                  backing ISafetyPoliciesRepository.GetSafetyPoliciesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicySafetyPolicies_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PolicyId],
        [TenantId],
        [SiteId],
        [PolicyName],
        [Category],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[PolicySafetyPolicies]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
GO