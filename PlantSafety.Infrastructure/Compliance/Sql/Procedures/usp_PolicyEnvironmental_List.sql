-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicyEnvironmental
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PolicyEnvironmental_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[PolicyEnvironmental],
--                  backing IEnvironmentalRepository.GetEnvironmentalPoliciesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyEnvironmental_List]
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
        [EnvironmentalAspect],
        [RegulatoryReference],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[PolicyEnvironmental]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
GO