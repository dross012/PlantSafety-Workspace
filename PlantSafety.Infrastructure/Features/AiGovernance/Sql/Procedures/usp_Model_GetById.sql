-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.AiGovernance
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Model_GetById.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IAiGovernanceRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AiGovernance].[usp_Model_GetById]
    @TenantId UNIQUEIDENTIFIER,
    @ModelId  UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ModelId,
        TenantId,
        SiteId,
        ModelName,
        ModelVersion,
        GovernanceStatus,
        RiskTier,
        DriftStatus,
        LastReviewedAtUtc,
        ReviewerName
    FROM [AiGovernance].[Model]
    WHERE TenantId = @TenantId
      AND ModelId = @ModelId
      AND IsDeleted = 0;
END
GO
