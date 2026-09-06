-- =============================================
-- Enterprise.Integration -- IncidentMgt-Integration
-- Script:      51-usp-validateintegration-extensions.sql
-- Purpose:     Extends usp_ValidateIncidentMgtIntegration (21-*.sql) with checks for the reads
--              procs (22-*.sql) and governance objects (50-*.sql). CREATE OR ALTERs the SAME
--              procedure -- deploy applies files in filename order, so this always runs after
--              21-*.sql and is the effective, final definition (matches
--              EmergencyMgt-Integration's own 51-usp-validateintegration-extensions.sql
--              precedent exactly).
-- Idempotent:  CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_ValidateIncidentMgtIntegration]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Checks TABLE (CheckName NVARCHAR(128) NOT NULL, Passed BIT NOT NULL, Detail NVARCHAR(400) NULL);

    -- Schema presence
    INSERT INTO @Checks
    SELECT N'Schema:IncidentMgtIntegration', CASE WHEN EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'IncidentMgtIntegration') THEN 1 ELSE 0 END, NULL;

    -- Table presence (4 original + VersionManifest/DriftCheck/ArchiveRegistry)
    INSERT INTO @Checks
    SELECT N'Table:RoutingKeyRegistry', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.RoutingKeyRegistry', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:IncidentSummaryFeed', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.IncidentSummaryFeed', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:GovernanceConsumptionLog', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.GovernanceConsumptionLog', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:OutboundEventRegistry', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.OutboundEventRegistry', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:VersionManifest', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.VersionManifest', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:DriftCheck', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.DriftCheck', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:ArchiveRegistry', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.ArchiveRegistry', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL;

    -- View presence (5)
    INSERT INTO @Checks
    SELECT N'View:vwIncidentDashboardReadOnly', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.vwIncidentDashboardReadOnly', N'V') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'View:vwIncidentTrendWidget', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.vwIncidentTrendWidget', N'V') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'View:vwSeverityDistribution', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.vwSeverityDistribution', N'V') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'View:vwCorrectiveActionStatusReadOnly', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.vwCorrectiveActionStatusReadOnly', N'V') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'View:vwInvestigationStatusReadOnly', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.vwInvestigationStatusReadOnly', N'V') IS NOT NULL THEN 1 ELSE 0 END, NULL;

    -- Procedure presence (7 original + 8 read procs + 7 governance procs)
    INSERT INTO @Checks
    SELECT N'Proc:usp_IncidentSummaryFeed_Upsert', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_IncidentSummaryFeed_Upsert', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_RoutingKeyRegistry_Register', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_RoutingKeyRegistry_Register', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_RoutingKeyRegistry_Heartbeat', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_RoutingKeyRegistry_Heartbeat', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_GovernanceConsumption_Record', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_GovernanceConsumption_Record', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_IntegrationHealth_Check', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_IntegrationHealth_Check', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_OutboundEventRegistry_Register', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_OutboundEventRegistry_Register', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_OutboundEventRegistry_Heartbeat', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_OutboundEventRegistry_Heartbeat', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_ValidateIncidentMgtIntegration', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_ValidateIncidentMgtIntegration', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_IncidentDashboard_GetAll', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_IncidentDashboard_GetAll', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_IncidentTrend_GetAll', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_IncidentTrend_GetAll', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_SeverityDistribution_GetAll', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_SeverityDistribution_GetAll', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_CorrectiveActionStatus_GetAll', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_CorrectiveActionStatus_GetAll', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_InvestigationStatus_GetAll', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_InvestigationStatus_GetAll', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_GovernanceConsumptionLog_GetLatestBySignalType', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_GovernanceConsumptionLog_GetLatestBySignalType', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_RoutingKeyRegistry_GetAll', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_RoutingKeyRegistry_GetAll', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_OutboundEventRegistry_GetAll', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_OutboundEventRegistry_GetAll', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_VersionManifest_ComputeAndRecord', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_VersionManifest_ComputeAndRecord', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_VersionManifest_SetSignature', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_VersionManifest_SetSignature', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_VersionManifest_GetLatest', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_VersionManifest_GetLatest', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_DriftCheck_Record', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_DriftCheck_Record', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_DriftCheck_GetUnresolved', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_DriftCheck_GetUnresolved', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_DriftCheck_Resolve', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_DriftCheck_Resolve', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Proc:usp_EnforceRetention', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_EnforceRetention', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL;

    -- Append-only guard triggers (2)
    INSERT INTO @Checks
    SELECT N'AppendOnlyGuard:VersionManifest', CASE WHEN EXISTS (SELECT 1 FROM sys.triggers WHERE name = N'TR_IncidentMgtIntegration_VersionManifest_NoDelete') THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'AppendOnlyGuard:DriftCheck', CASE WHEN EXISTS (SELECT 1 FROM sys.triggers WHERE name = N'TR_IncidentMgtIntegration_DriftCheck_NoDelete') THEN 1 ELSE 0 END, NULL;

    -- Routing key seed (8 keys registered by 40-seed-routingkeys.sql)
    INSERT INTO @Checks
    SELECT N'RoutingKeySeed', CASE WHEN (SELECT COUNT(*) FROM [IncidentMgtIntegration].[RoutingKeyRegistry] WHERE [IsActive] = 1) = 8 THEN 1 ELSE 0 END,
           CONCAT(N'active count=', (SELECT COUNT(*) FROM [IncidentMgtIntegration].[RoutingKeyRegistry] WHERE [IsActive] = 1), N' (expected 8)');

    -- Outbound feed registry (5 feeds registered by 02-outbound-tables.sql)
    INSERT INTO @Checks
    SELECT N'OutboundFeedSeed', CASE WHEN (SELECT COUNT(*) FROM [IncidentMgtIntegration].[OutboundEventRegistry] WHERE [IsActive] = 1) = 5 THEN 1 ELSE 0 END,
           CONCAT(N'active count=', (SELECT COUNT(*) FROM [IncidentMgtIntegration].[OutboundEventRegistry] WHERE [IsActive] = 1), N' (expected 5)');

    -- Drift detection (zero unresolved findings, matching every other module's own convention)
    INSERT INTO @Checks
    SELECT N'DriftDetection', CASE WHEN (SELECT COUNT(*) FROM [IncidentMgtIntegration].[DriftCheck] WHERE IsResolved = 0) = 0 THEN 1 ELSE 0 END,
           CONCAT(N'unresolved count=', (SELECT COUNT(*) FROM [IncidentMgtIntegration].[DriftCheck] WHERE IsResolved = 0));

    -- Version manifest population (distinguishes object presence, above, from actually having a
    -- recorded row -- matching EmergencyMgt-Integration's own convention).
    INSERT INTO @Checks
    SELECT N'VersionManifestPopulated', CASE WHEN EXISTS (SELECT 1 FROM [IncidentMgtIntegration].[VersionManifest]) THEN 1 ELSE 0 END,
           CONCAT(N'row count=', (SELECT COUNT(*) FROM [IncidentMgtIntegration].[VersionManifest]));

    -- Informational data-population checks -- honestly reported, never a false pass/fail on
    -- "no automatic puller/pusher exists yet" (same convention as EmergencyMgt-Integration's
    -- own DataFlow:* checks).
    INSERT INTO @Checks
    SELECT N'DataFlow:IncidentSummaryFeed', 1,
           CONCAT(N'row count=', (SELECT COUNT(*) FROM [IncidentMgtIntegration].[IncidentSummaryFeed]), N' -- informational only; no automatic IncidentMgt-side puller is wired yet, so 0 is an expected, honest value, not a failure');
    INSERT INTO @Checks
    SELECT N'DataFlow:GovernanceConsumptionLog', 1,
           CONCAT(N'row count=', (SELECT COUNT(*) FROM [IncidentMgtIntegration].[GovernanceConsumptionLog]), N' -- informational only; see DataFlow:IncidentSummaryFeed');

    SELECT CheckName, Passed, Detail FROM @Checks ORDER BY CheckName;
END
GO
