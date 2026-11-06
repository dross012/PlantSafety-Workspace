-- =============================================
-- Enterprise.Integration -- EmergencyMgt-Integration
-- Script:      21-usp-validateintegration.sql
-- Purpose:     usp_ValidateEmergencyMgtIntegration -- installer-contract validation proc for
--              this integration surface, mirroring EmergencyMgt's own
--              Governance.usp_ValidateInstallation shape (CheckName/Passed/Detail columns, one
--              independent finding per row, ORDER BY CheckName) and IncidentMgt-Integration's
--              own usp_IntegrationHealth_Check precedent. Every check here asserts SCHEMA/OBJECT
--              presence (real, checkable today); a handful of DATA-population checks are
--              reported as informational (Passed=1 with an honest zero-count Detail) rather than
--              hard failures, since no automatic puller exists on the EmergencyMgt side yet
--              (EmergencyMgt Governance.IntegrationContractRegistration records PlantSafety as
--              DeferredSeamOnly) -- failing on "no data received yet" would conflate "this
--              schema is correctly installed" with "a live feed is currently flowing", which
--              are two different, honestly distinguishable facts.
-- Idempotent:  CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_ValidateEmergencyMgtIntegration]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Checks TABLE (CheckName NVARCHAR(128) NOT NULL, Passed BIT NOT NULL, Detail NVARCHAR(400) NULL);

    -- Schema presence
    INSERT INTO @Checks
    SELECT N'Schema:EmergencyMgtIntegration', CASE WHEN EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'EmergencyMgtIntegration') THEN 1 ELSE 0 END, NULL;

    -- Table presence (6)
    INSERT INTO @Checks
    SELECT N'Table:RoutingKeyRegistry', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.RoutingKeyRegistry', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:EmergencySummaryInbound', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.EmergencySummaryInbound', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:EmergencyLifecycleInbound', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.EmergencyLifecycleInbound', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:EmergencyCorrectiveActionInbound', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.EmergencyCorrectiveActionInbound', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:EmergencyReadinessInbound', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.EmergencyReadinessInbound', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:EmergencyGovernanceLog', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.EmergencyGovernanceLog', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL;

    -- View presence (5)
    INSERT INTO @Checks
    SELECT N'View:vwEmergencySummary', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.vwEmergencySummary', N'V') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'View:vwEmergencyLifecycle', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.vwEmergencyLifecycle', N'V') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'View:vwEmergencyCorrectiveActions', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.vwEmergencyCorrectiveActions', N'V') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'View:vwEmergencyReadiness', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.vwEmergencyReadiness', N'V') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'View:vwEmergencyKPIs', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.vwEmergencyKPIs', N'V') IS NOT NULL THEN 1 ELSE 0 END, NULL;

    -- Procedure presence (9)
    INSERT INTO @Checks
    SELECT N'Proc:usp_EmergencySummaryInbound_Upsert', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_EmergencySummaryInbound_Upsert', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_EmergencyLifecycleInbound_Record', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_EmergencyLifecycleInbound_Record', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_EmergencyCorrectiveActionInbound_Upsert', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_EmergencyCorrectiveActionInbound_Upsert', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_EmergencyReadinessInbound_Upsert', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_EmergencyReadinessInbound_Upsert', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_RoutingKeyRegistry_Register', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_RoutingKeyRegistry_Register', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_RoutingKeyRegistry_Heartbeat', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_RoutingKeyRegistry_Heartbeat', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_EmergencyGovernanceLog_Record', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_EmergencyGovernanceLog_Record', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_IntegrationHealth_Check', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_IntegrationHealth_Check', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_ValidateEmergencyMgtIntegration', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_ValidateEmergencyMgtIntegration', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL;

    -- Routing key seed (20 keys registered by 40-seed-routingkeys.sql)
    INSERT INTO @Checks
    SELECT N'RoutingKeySeed', CASE WHEN (SELECT COUNT(*) FROM [EmergencyMgtIntegration].[RoutingKeyRegistry] WHERE [IsActive] = 1) = 20 THEN 1 ELSE 0 END,
           CONCAT(N'active count=', (SELECT COUNT(*) FROM [EmergencyMgtIntegration].[RoutingKeyRegistry] WHERE [IsActive] = 1), N' (expected 20)');

    -- Informational data-population checks -- honestly reported, never a false pass/fail on
    -- "no automatic puller exists yet" (see file header).
    INSERT INTO @Checks
    SELECT N'DataFlow:EmergencySummaryInbound', 1,
           CONCAT(N'row count=', (SELECT COUNT(*) FROM [EmergencyMgtIntegration].[EmergencySummaryInbound]), N' -- informational only; no automatic EmergencyMgt-side puller is wired yet (DeferredSeamOnly), so 0 is an expected, honest value, not a failure');
    INSERT INTO @Checks
    SELECT N'DataFlow:EmergencyGovernanceLog', 1,
           CONCAT(N'row count=', (SELECT COUNT(*) FROM [EmergencyMgtIntegration].[EmergencyGovernanceLog]), N' -- informational only; see DataFlow:EmergencySummaryInbound');

    SELECT CheckName, Passed, Detail FROM @Checks ORDER BY CheckName;
END
GO
