-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPolicies
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_SafetyPolicies_GetByCategory.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Shared read-model stored procedure backing all 17 SafetyPolicies category
--                  Application-layer repositories (IConfinedSpaceEntryRepository,
--                  IContractorSafetyRepository, IElectricalSafetyRepository, etc). Each category
--                  repository method calls this same procedure with its own literal @Category
--                  value (matching the PolicyCategory.Code for that category), avoiding 17
--                  near-duplicate stored procedures for what is functionally one filtered query
--                  shape over the SafetyPolicy table.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyPolicies].[usp_SafetyPolicies_GetByCategory]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3),
    @Category NVARCHAR(100)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        SafetyPolicyId,
        TenantId,
        SiteId,
        SafetyPolicyTitle AS Title,
        Category,
        Status,
        ISNULL(EffectiveAtUtc, CreatedAtUtc) AS EffectiveAtUtc,
        OwnerId
    FROM [SafetyPolicies].[SafetyPolicy]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND Category = @Category
      AND IsDeleted = 0
      AND ISNULL(EffectiveAtUtc, CreatedAtUtc) >= @FromUtc
      AND ISNULL(EffectiveAtUtc, CreatedAtUtc) <= @ToUtc
    ORDER BY ISNULL(EffectiveAtUtc, CreatedAtUtc) DESC;
END
GO
