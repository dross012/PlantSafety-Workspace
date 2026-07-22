-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPolicies
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PolicyAcknowledgment_DeleteByPolicyId.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Soft-deletes all PolicyAcknowledgment rows for a SafetyPolicyId, used by
--                  ISafetyPolicyRepository.UpdateAsync to reconcile the in-memory Acknowledgments
--                  collection (delete-then-reinsert strategy) whenever the aggregate's published
--                  revision is republished/reset (Publish/CompleteReview clear the collection).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyPolicies].[usp_PolicyAcknowledgment_DeleteByPolicyId]
    @SafetyPolicyId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    UPDATE [SafetyPolicies].[PolicyAcknowledgment]
    SET IsDeleted = 1
    WHERE SafetyPolicyId = @SafetyPolicyId AND IsDeleted = 0;
END
GO
