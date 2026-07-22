-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.ActiveMonitoringAuditing
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ActiveMonitoringAuditing_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single ActiveMonitoringAuditing row by identifier, backing
--                  IActiveMonitoringAuditingRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_ActiveMonitoringAuditing_GetById]
(
    @ActiveMonitoringAuditingId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [ActiveMonitoringAuditingId], [TenantId], [SiteId], [Status], [MonitoringTitle], [Category], [WorkLocation], [MonitoredByUserId], [ObservedAtUtc], [UnsafeConditionCount], [ComplianceScore], [FindingNotes], [FollowUpAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [ContractorSafety].[ActiveMonitoringAuditing]
    WHERE [ActiveMonitoringAuditingId] = @ActiveMonitoringAuditingId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
