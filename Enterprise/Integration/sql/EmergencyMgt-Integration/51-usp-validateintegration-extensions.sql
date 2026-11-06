-- =============================================
-- Enterprise.Integration -- EmergencyMgt-Integration
-- Script:      51-usp-validateintegration-extensions.sql
-- Purpose:     Extends usp_ValidateEmergencyMgtIntegration (21-*.sql) with governance-object
--              checks for the objects added in 50-governance.sql. CREATE OR ALTERs the SAME
--              procedure -- deploy applies files in filename order, so this always runs after
--              21-*.sql and is the effective, final definition (matches the EmergencyMgt
--              precedent of 54-usp-validateinstallation-extensions.sql extending 52-*.sql).
--              Also updates the DataFlow:* checks' wording now that
--              PlantSafety.Integration.Pusher.Local has been run live and genuinely populated
--              these tables -- 0 rows is no longer the universally-expected case, but a real
--              row count still is not itself a pass/fail signal (a quiet period with no new
--              EmergencyMgt events is not a failure), so these remain informational.
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

    -- Table presence (9: 6 original + VersionManifest/DriftCheck/ArchiveRegistry)
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
    SELECT N'Table:EmergencyGovernanceLog', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.EmergencyGovernanceLog', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:VersionManifest', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.VersionManifest', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:DriftCheck', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.DriftCheck', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:ArchiveRegistry', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.ArchiveRegistry', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL;

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

    -- Procedure presence (9 original + 7 read procs + 7 governance procs)
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
    SELECT N'Proc:usp_ValidateEmergencyMgtIntegration', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_ValidateEmergencyMgtIntegration', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_EmergencySummary_GetAll', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_EmergencySummary_GetAll', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_EmergencyLifecycle_GetAll', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_EmergencyLifecycle_GetAll', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_EmergencyCorrectiveActions_GetAll', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_EmergencyCorrectiveActions_GetAll', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_EmergencyReadiness_GetAll', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_EmergencyReadiness_GetAll', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_EmergencyKpis_GetAll', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_EmergencyKpis_GetAll', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_EmergencyGovernanceLog_GetLatestBySignalType', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_EmergencyGovernanceLog_GetLatestBySignalType', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_RoutingKeyRegistry_GetAll', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_RoutingKeyRegistry_GetAll', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_VersionManifest_ComputeAndRecord', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_VersionManifest_ComputeAndRecord', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_VersionManifest_SetSignature', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_VersionManifest_SetSignature', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_VersionManifest_GetLatest', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_VersionManifest_GetLatest', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_DriftCheck_Record', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_DriftCheck_Record', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_DriftCheck_GetUnresolved', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_DriftCheck_GetUnresolved', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_DriftCheck_Resolve', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_DriftCheck_Resolve', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_EnforceRetention', CASE WHEN OBJECT_ID(N'EmergencyMgtIntegration.usp_EnforceRetention', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL;

    -- Append-only guard triggers (2 new)
    INSERT INTO @Checks
    SELECT N'AppendOnlyGuard:VersionManifest', CASE WHEN EXISTS (SELECT 1 FROM sys.triggers WHERE name = N'TR_EmergencyMgtIntegration_VersionManifest_NoDelete') THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'AppendOnlyGuard:DriftCheck', CASE WHEN EXISTS (SELECT 1 FROM sys.triggers WHERE name = N'TR_EmergencyMgtIntegration_DriftCheck_NoDelete') THEN 1 ELSE 0 END, NULL;

    -- Routing key seed (20 keys registered by 40-seed-routingkeys.sql)
    INSERT INTO @Checks
    SELECT N'RoutingKeySeed', CASE WHEN (SELECT COUNT(*) FROM [EmergencyMgtIntegration].[RoutingKeyRegistry] WHERE [IsActive] = 1) = 20 THEN 1 ELSE 0 END,
           CONCAT(N'active count=', (SELECT COUNT(*) FROM [EmergencyMgtIntegration].[RoutingKeyRegistry] WHERE [IsActive] = 1), N' (expected 20)');

    -- Drift detection (zero unresolved findings, matching every other module's own convention)
    INSERT INTO @Checks
    SELECT N'DriftDetection', CASE WHEN (SELECT COUNT(*) FROM [EmergencyMgtIntegration].[DriftCheck] WHERE IsResolved = 0) = 0 THEN 1 ELSE 0 END,
           CONCAT(N'unresolved count=', (SELECT COUNT(*) FROM [EmergencyMgtIntegration].[DriftCheck] WHERE IsResolved = 0));

    -- Version manifest population (distinguishes object presence, above, from actually having a
    -- recorded row -- matching EmergencyMgt's own usp_ValidateInstallation header convention).
    INSERT INTO @Checks
    SELECT N'VersionManifestPopulated', CASE WHEN EXISTS (SELECT 1 FROM [EmergencyMgtIntegration].[VersionManifest]) THEN 1 ELSE 0 END,
           CONCAT(N'row count=', (SELECT COUNT(*) FROM [EmergencyMgtIntegration].[VersionManifest]));

    -- Informational data-flow checks -- now genuinely populated by PlantSafety.Integration.
    -- Pusher.Local's live runs (see Enterprise/Integration/Pusher), but still reported as
    -- informational rather than hard pass/fail: a quiet period with no new EmergencyMgt events
    -- is not itself a failure, and the pusher is a manually/scheduled-run tool, not a continuous
    -- automatic feed (no message bus exists to make it one -- see this folder's README.md).
    INSERT INTO @Checks
    SELECT N'DataFlow:EmergencySummaryInbound', 1,
           CONCAT(N'row count=', (SELECT COUNT(*) FROM [EmergencyMgtIntegration].[EmergencySummaryInbound]), N' -- informational only; PlantSafety.Integration.Pusher.Local pushes on manual/scheduled invocation, not continuously (no message bus exists)');
    INSERT INTO @Checks
    SELECT N'DataFlow:EmergencyGovernanceLog', 1,
           CONCAT(N'row count=', (SELECT COUNT(*) FROM [EmergencyMgtIntegration].[EmergencyGovernanceLog]), N' -- informational only; see DataFlow:EmergencySummaryInbound');

    SELECT CheckName, Passed, Detail FROM @Checks ORDER BY CheckName;
END
GO
