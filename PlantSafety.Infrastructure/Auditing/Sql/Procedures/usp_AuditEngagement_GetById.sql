-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditEngagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AuditEngagement_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing IAuditEngagementRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Auditing].[usp_AuditEngagement_GetById]
    @AuditEngagementId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [AuditEngagementId], [TenantId], [SiteId], [Status], [EngagementTitle], [DisciplineType],
    [Scope], [LeadAuditorId], [ScheduledAtUtc], [CompletedAtUtc], [FindingsCount],
    [ComplianceScore], [ComplianceRating], [CreatedAtUtc], [CreatedBy]
FROM [Auditing].[AuditEngagement]
WHERE [AuditEngagementId] = @AuditEngagementId
  AND ISNULL([IsDeleted], 0) = 0;
GO
