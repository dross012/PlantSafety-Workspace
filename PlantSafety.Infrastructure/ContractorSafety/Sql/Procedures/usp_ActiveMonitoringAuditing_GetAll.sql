-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.ActiveMonitoringAuditing
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ActiveMonitoringAuditing_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted ActiveMonitoringAuditing rows, backing
--                  IActiveMonitoringAuditingRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_ActiveMonitoringAuditing_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [ActiveMonitoringAuditingId], [TenantId], [SiteId], [Status], [MonitoringTitle], [Category], [WorkLocation], [MonitoredByUserId], [ObservedAtUtc], [UnsafeConditionCount], [ComplianceScore], [FindingNotes], [FollowUpAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [ContractorSafety].[ActiveMonitoringAuditing]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
