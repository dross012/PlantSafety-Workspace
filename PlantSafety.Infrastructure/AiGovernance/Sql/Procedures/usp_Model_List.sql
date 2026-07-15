-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.AiGovernance
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Model_List.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns the governed AI/ML model registry for a tenant. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AiGovernance].[usp_Model_List]
(
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [ModelId], [TenantId], [SiteId], [ModelName], [ModelVersion], [GovernanceStatus],
           [RiskTier], [DriftStatus], [LastReviewedAtUtc], [ReviewerName]
    FROM [AiGovernance].[GovernedModel]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
    ORDER BY [LastReviewedAtUtc] DESC;
END;
GO
