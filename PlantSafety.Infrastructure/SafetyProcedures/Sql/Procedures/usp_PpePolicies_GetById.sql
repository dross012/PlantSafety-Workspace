-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.PpePolicies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PpePolicies_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single PpePolicies row by identifier, backing
--                  IPpePoliciesRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_PpePolicies_GetById]
(
    @PpePoliciesId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [PpePoliciesId], [TenantId], [SiteId], [Status], [PolicyTitle], [PpeCategory], [AssignedZone], [ApprovedByUserId], [PublishedAtUtc], [NextReviewDueAtUtc], [ReplacementScheduleCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[PpePolicies]
    WHERE [PpePoliciesId] = @PpePoliciesId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
