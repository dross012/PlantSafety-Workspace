-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPolicies
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PolicyAcknowledgment_GetByPolicyId.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all PolicyAcknowledgment child rows for a given SafetyPolicyId.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyPolicies].[usp_PolicyAcknowledgment_GetByPolicyId]
    @SafetyPolicyId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT AcknowledgmentId, SafetyPolicyId, AcknowledgedPolicyVersion, UserId, AcknowledgedAtUtc
    FROM [SafetyPolicies].[PolicyAcknowledgment]
    WHERE SafetyPolicyId = @SafetyPolicyId AND IsDeleted = 0
    ORDER BY AcknowledgedAtUtc ASC;
END
GO
