-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.PpePolicies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PpePolicies_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PpePolicies rows, backing
--                  IPpePoliciesRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_PpePolicies_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [PpePoliciesId], [TenantId], [SiteId], [Status], [PolicyTitle], [PpeCategory], [AssignedZone], [ApprovedByUserId], [PublishedAtUtc], [NextReviewDueAtUtc], [ReplacementScheduleCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[PpePolicies]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
