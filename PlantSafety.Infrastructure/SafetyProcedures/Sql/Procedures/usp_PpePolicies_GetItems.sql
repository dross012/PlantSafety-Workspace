-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.PpePolicies
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PpePolicies_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IPpePoliciesRepository.GetPpePoliciesItemsAsync (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_PpePolicies_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [PpePoliciesId],
        [TenantId],
        [SiteId],
        [PolicyTitle],
        [PpeCategory] AS [PpeCategory],
        [AssignedZone],
        [Status] AS [PolicyStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [SafetyProcedures].[PpePolicies]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO
