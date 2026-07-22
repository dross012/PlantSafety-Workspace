-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditFinding
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AuditFinding_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing IAuditFindingRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Auditing].[usp_AuditFinding_GetById]
    @AuditFindingId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [AuditFindingId], [TenantId], [SiteId], [Status], [AuditEngagementId], [FindingTitle],
    [Severity], [CorrectiveActionRequired], [AssignedToUserId], [DueDateUtc], [VerifiedAtUtc],
    [RemediationNotes], [RemediatedAtUtc], [CreatedAtUtc], [CreatedBy]
FROM [Auditing].[AuditFinding]
WHERE [AuditFindingId] = @AuditFindingId
  AND ISNULL([IsDeleted], 0) = 0;
GO
