-- =============================================
-- Enterprise.Integration -- IncidentMgt-Integration
-- Script:      21-usp-validateintegration.sql
-- Purpose:     usp_ValidateIncidentMgtIntegration -- installer-contract validation proc for
--              this integration surface, mirroring EmergencyMgt-Integration's own
--              usp_ValidateEmergencyMgtIntegration shape exactly (CheckName/Passed/Detail
--              columns, one independent finding per row, ORDER BY CheckName). This module was
--              deployed to the live PlantSafety.CoreDB for the first time alongside this file
--              (see sql/DEPLOYMENT-MANIFEST.md) -- prior to this it was source-only.
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

    -- Table presence (4)
    INSERT INTO @Checks
    SELECT N'Table:RoutingKeyRegistry', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.RoutingKeyRegistry', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:IncidentSummaryFeed', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.IncidentSummaryFeed', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:GovernanceConsumptionLog', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.GovernanceConsumptionLog', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL
    UNION ALL
    SELECT N'Table:OutboundEventRegistry', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.OutboundEventRegistry', N'U') IS NOT NULL THEN 1 ELSE 0 END, NULL;

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

    -- Procedure presence (7 from 02/20, plus this proc itself)
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
    SELECT N'Proc:usp_ValidateIncidentMgtIntegration', CASE WHEN OBJECT_ID(N'IncidentMgtIntegration.usp_ValidateIncidentMgtIntegration', N'P') IS NOT NULL THEN 1 ELSE 0 END, NULL;

    -- Routing key seed (8 keys registered by 40-seed-routingkeys.sql)
    INSERT INTO @Checks
    SELECT N'RoutingKeySeed', CASE WHEN (SELECT COUNT(*) FROM [IncidentMgtIntegration].[RoutingKeyRegistry] WHERE [IsActive] = 1) = 8 THEN 1 ELSE 0 END,
           CONCAT(N'active count=', (SELECT COUNT(*) FROM [IncidentMgtIntegration].[RoutingKeyRegistry] WHERE [IsActive] = 1), N' (expected 8)');

    -- Outbound feed registry (5 feeds registered by 02-outbound-tables.sql)
    INSERT INTO @Checks
    SELECT N'OutboundFeedSeed', CASE WHEN (SELECT COUNT(*) FROM [IncidentMgtIntegration].[OutboundEventRegistry] WHERE [IsActive] = 1) = 5 THEN 1 ELSE 0 END,
           CONCAT(N'active count=', (SELECT COUNT(*) FROM [IncidentMgtIntegration].[OutboundEventRegistry] WHERE [IsActive] = 1), N' (expected 5)');

    -- Informational data-population checks -- honestly reported, never a false pass/fail on
    -- "no automatic puller/pusher exists yet" (same convention as EmergencyMgt-Integration's
    -- own DataFlow:* checks): this schema is correctly installed regardless of whether a live
    -- feed happens to be flowing at validation time, and those are two different facts.
    INSERT INTO @Checks
    SELECT N'DataFlow:IncidentSummaryFeed', 1,
           CONCAT(N'row count=', (SELECT COUNT(*) FROM [IncidentMgtIntegration].[IncidentSummaryFeed]), N' -- informational only; no automatic IncidentMgt-side puller is wired yet, so 0 is an expected, honest value, not a failure');
    INSERT INTO @Checks
    SELECT N'DataFlow:GovernanceConsumptionLog', 1,
           CONCAT(N'row count=', (SELECT COUNT(*) FROM [IncidentMgtIntegration].[GovernanceConsumptionLog]), N' -- informational only; see DataFlow:IncidentSummaryFeed');

    SELECT CheckName, Passed, Detail FROM @Checks ORDER BY CheckName;
END
GO
