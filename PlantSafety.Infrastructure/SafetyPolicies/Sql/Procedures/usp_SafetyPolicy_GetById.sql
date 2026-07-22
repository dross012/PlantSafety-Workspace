-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPolicies
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_SafetyPolicy_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single SafetyPolicy aggregate root row by identity. Backs
--                  ISafetyPolicyRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyPolicies].[usp_SafetyPolicy_GetById]
    @SafetyPolicyId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        SafetyPolicyId, TenantId, SiteId, Status, SafetyPolicyTitle, PolicyNumber, SafetyPolicyVersion,
        Category, Applicability, OwnerId, ApprovedBy, ApprovedAtUtc, EffectiveAtUtc, ReviewCadence,
        LastReviewedAtUtc, NextReviewDueUtc, ReviewRequired, RequiredAcknowledgmentCount, ReviewCount,
        CreatedAtUtc, CreatedBy
    FROM [SafetyPolicies].[SafetyPolicy]
    WHERE SafetyPolicyId = @SafetyPolicyId AND IsDeleted = 0;
END
GO
