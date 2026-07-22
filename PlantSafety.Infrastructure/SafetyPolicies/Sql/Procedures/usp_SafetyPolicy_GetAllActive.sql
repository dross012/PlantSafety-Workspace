-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPolicies
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_SafetyPolicy_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted SafetyPolicy aggregate root rows. Backs
--                  ISafetyPolicyRepository.ListAsync, whose SafetyPolicySpecification predicate
--                  is applied in-memory over the candidate set returned here (the domain
--                  specification is an Expression<Func<SafetyPolicy,bool>>, evaluated in the
--                  Infrastructure repository after rehydration -- no TenantId parameter is
--                  available on this call because the ListAsync() interface signature the
--                  Infrastructure layer must implement does not carry one).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyPolicies].[usp_SafetyPolicy_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        SafetyPolicyId, TenantId, SiteId, Status, SafetyPolicyTitle, PolicyNumber, SafetyPolicyVersion,
        Category, Applicability, OwnerId, ApprovedBy, ApprovedAtUtc, EffectiveAtUtc, ReviewCadence,
        LastReviewedAtUtc, NextReviewDueUtc, ReviewRequired, RequiredAcknowledgmentCount, ReviewCount,
        CreatedAtUtc, CreatedBy
    FROM [SafetyPolicies].[SafetyPolicy]
    WHERE IsDeleted = 0;
END
GO
