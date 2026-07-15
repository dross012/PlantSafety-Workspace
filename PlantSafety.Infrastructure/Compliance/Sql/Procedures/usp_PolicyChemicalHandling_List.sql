-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicyChemicalHandling
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PolicyChemicalHandling_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[PolicyChemicalHandling],
--                  backing IChemicalHandlingRepository.GetChemicalHandlingPoliciesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyChemicalHandling_List]
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
        [ChemicalName],
        [HazardClass],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[PolicyChemicalHandling]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
GO