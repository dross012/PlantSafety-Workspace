-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditEngagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AuditEngagement_ListByTenant.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing IAuditEngagementRepository.ListAsync. Returns
--                  candidate header rows for the tenant/site; AuditEngagementSpecification
--                  predicates are applied in-memory in the repository after rehydration.
--                  Additive-only evolution.
--  UPDATED:        2026-08-02 (Phase 6 query-plan-hint pass) -- added
--                  OPTION (OPTIMIZE FOR UNKNOWN) per
--                  ..\..\Analytics\UnifiedModel\Sql\query-plan-hint-convention.md:
--                  IX_AuditEngagement_TenantId_SiteId has SiteId as a genuine key column (not
--                  just INCLUDE), so an all-sites call (@SiteId IS NULL, TenantId-prefix seek)
--                  and a single-site call (full two-column seek) are two different seek shapes;
--                  OPTIMIZE FOR UNKNOWN avoids caching a skewed plan from an unlucky first
--                  caller without paying OPTION (RECOMPILE)'s per-call cost on this
--                  higher-frequency list endpoint.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Auditing].[usp_AuditEngagement_ListByTenant]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
SELECT
    [AuditEngagementId], [TenantId], [SiteId], [Status], [EngagementTitle], [DisciplineType],
    [Scope], [LeadAuditorId], [ScheduledAtUtc], [CompletedAtUtc], [FindingsCount],
    [ComplianceScore], [ComplianceRating], [CreatedAtUtc], [CreatedBy]
FROM [Auditing].[AuditEngagement]
WHERE [TenantId] = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND ISNULL([IsDeleted], 0) = 0
ORDER BY [CreatedAtUtc] DESC
OPTION (OPTIMIZE FOR UNKNOWN);
GO
