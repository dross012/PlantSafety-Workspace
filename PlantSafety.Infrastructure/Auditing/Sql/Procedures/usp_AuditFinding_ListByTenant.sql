-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditFinding
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AuditFinding_ListByTenant.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing IAuditFindingRepository.ListAsync. Returns
--                  candidate header rows for the tenant/site; AuditFindingSpecification
--                  predicates are applied in-memory in the repository after rehydration.
--                  Additive-only evolution.
--  UPDATED:        2026-08-02 (Phase 6 query-plan-hint pass) -- added
--                  OPTION (OPTIMIZE FOR UNKNOWN) per
--                  ..\..\Analytics\UnifiedModel\Sql\query-plan-hint-convention.md, for
--                  consistency with the identically-shaped optional @SiteId filter on the
--                  sibling usp_AuditEngagement_ListByTenant proc over the same [Auditing]
--                  schema; avoids caching a skewed plan from an unlucky first caller without
--                  paying OPTION (RECOMPILE)'s per-call cost on this list endpoint.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Auditing].[usp_AuditFinding_ListByTenant]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
SELECT
    [AuditFindingId], [TenantId], [SiteId], [Status], [AuditEngagementId], [FindingTitle],
    [Severity], [CorrectiveActionRequired], [AssignedToUserId], [DueDateUtc], [VerifiedAtUtc],
    [RemediationNotes], [RemediatedAtUtc], [CreatedAtUtc], [CreatedBy]
FROM [Auditing].[AuditFinding]
WHERE [TenantId] = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND ISNULL([IsDeleted], 0) = 0
ORDER BY [CreatedAtUtc] DESC
OPTION (OPTIMIZE FOR UNKNOWN);
GO
