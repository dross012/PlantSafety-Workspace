-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         GlobalSafetyInsights
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CrossSiteActionLog_ListByTenant.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 8 §5 Global Safety Dashboard)
--  SchemaVersion:  2026.1
--  Description:    Retrieves cross-site coordinated-action audit ledger entries for TenantId
--                  within [@FromUtc, @ToUtc], most recent first. Backs
--                  IGlobalSafetyActionAuditRepository.ListAsync -- the Global Safety Dashboard's
--                  corrective-action tracker / incident-timeline widgets (Phase 8 §5).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [GlobalSafetyInsights].[usp_CrossSiteActionLog_ListByTenant]
    @TenantId UNIQUEIDENTIFIER,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CrossSiteActionLogId,
        TenantId,
        AffectedSiteIdsJson,
        ActionType,
        Description,
        InitiatedByUserId,
        RecordedAtUtc,
        RegionCode,
        WorkflowInstanceId,
        CorrelationId,
        CausationId
    FROM [GlobalSafetyInsights].[CrossSiteActionLog]
    WHERE TenantId = @TenantId
      AND RecordedAtUtc >= @FromUtc
      AND RecordedAtUtc <= @ToUtc
    ORDER BY RecordedAtUtc DESC;
END
GO
