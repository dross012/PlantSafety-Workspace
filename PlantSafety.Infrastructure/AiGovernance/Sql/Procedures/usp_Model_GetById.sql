-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.AiGovernance
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Model_GetById.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single governed AI/ML model by identifier. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AiGovernance].[usp_Model_GetById]
(
    @TenantId UNIQUEIDENTIFIER,
    @ModelId  UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [ModelId], [TenantId], [SiteId], [ModelName], [ModelVersion], [GovernanceStatus],
           [RiskTier], [DriftStatus], [LastReviewedAtUtc], [ReviewerName]
    FROM [AiGovernance].[GovernedModel]
    WHERE [TenantId] = @TenantId AND [ModelId] = @ModelId;
END;
GO
