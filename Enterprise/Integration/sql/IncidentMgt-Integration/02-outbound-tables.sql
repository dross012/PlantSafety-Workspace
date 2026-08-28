-- =============================================
-- Enterprise.Integration -- IncidentMgt-Integration
-- Script:  02-outbound-tables.sql
-- Purpose: OutboundEventRegistry -- records the 5 PlantSafety -> IncidentMgt outbound feeds
--          (Section 3.1): Safety alerts -> IncidentMgt escalation, Safety checklists ->
--          IncidentMgt evidence, Safety dashboards -> IncidentMgt summary feeds, Safety culture
--          -> IncidentMgt risk factors, Safety leading indicators -> IncidentMgt forecasting.
--          PlantSafety is the publisher here; IncidentMgt.Notification/Escalation/Evidence/
--          RiskMatrix/Forecast schemas are the consumers (see IncidentMgt repo). This table is
--          PlantSafety's local record of what it publishes and when it last did so -- the
--          mirror-image of RoutingKeyRegistry (01-tables.sql), which records what PlantSafety
--          *consumes* from IncidentMgt.
-- Idempotent: existence-checked CREATE. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'IncidentMgtIntegration' AND t.name = 'OutboundEventRegistry')
BEGIN
    CREATE TABLE [IncidentMgtIntegration].[OutboundEventRegistry] (
        [OutboundEventRegistryId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IncidentMgtIntegration_OutboundEventRegistry_Id DEFAULT NEWID(),
        [FeedName]         NVARCHAR(100) NOT NULL,
        [SourceCapability] NVARCHAR(100) NOT NULL,   -- e.g. 'Safety Alerts', 'Safety Checklists'
        [TargetCapability] NVARCHAR(100) NOT NULL,   -- e.g. 'Escalation.EscalationTrigger'
        [Description]      NVARCHAR(300) NULL,
        [IsActive]         BIT NOT NULL CONSTRAINT DF_IncidentMgtIntegration_OutboundEventRegistry_IsActive DEFAULT 1,
        [LastPublishedUtc] DATETIME2 NULL,
        CONSTRAINT PK_IncidentMgtIntegration_OutboundEventRegistry PRIMARY KEY CLUSTERED ([OutboundEventRegistryId]),
        CONSTRAINT UQ_IncidentMgtIntegration_OutboundEventRegistry_FeedName UNIQUE ([FeedName])
    );
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_OutboundEventRegistry_Register]
    @FeedName         NVARCHAR(100),
    @SourceCapability NVARCHAR(100),
    @TargetCapability NVARCHAR(100),
    @Description      NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        MERGE [IncidentMgtIntegration].[OutboundEventRegistry] AS target
        USING (SELECT @FeedName AS FeedName) AS src
            ON target.[FeedName] = src.FeedName
        WHEN MATCHED THEN
            UPDATE SET [SourceCapability] = @SourceCapability, [TargetCapability] = @TargetCapability,
                       [Description] = ISNULL(@Description, target.[Description]), [IsActive] = 1
        WHEN NOT MATCHED THEN
            INSERT ([FeedName], [SourceCapability], [TargetCapability], [Description])
            VALUES (@FeedName, @SourceCapability, @TargetCapability, @Description);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_OutboundEventRegistry_Heartbeat]
    @FeedName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        UPDATE [IncidentMgtIntegration].[OutboundEventRegistry]
        SET [LastPublishedUtc] = SYSUTCDATETIME()
        WHERE [FeedName] = @FeedName;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

EXEC [IncidentMgtIntegration].[usp_OutboundEventRegistry_Register]
    @FeedName = N'SafetyAlerts_To_Escalation', @SourceCapability = N'Safety Alerts',
    @TargetCapability = N'Escalation.EscalationTrigger', @Description = N'High-priority PlantSafety alerts open an IncidentMgt escalation trigger.';
EXEC [IncidentMgtIntegration].[usp_OutboundEventRegistry_Register]
    @FeedName = N'SafetyChecklists_To_Evidence', @SourceCapability = N'Safety Checklists',
    @TargetCapability = N'Evidence.EvidenceCustodyRecord', @Description = N'Completed safety checklists attach as investigation evidence.';
EXEC [IncidentMgtIntegration].[usp_OutboundEventRegistry_Register]
    @FeedName = N'SafetyDashboards_To_SummaryFeed', @SourceCapability = N'Safety Dashboards',
    @TargetCapability = N'IncidentMgtIntegration.IncidentSummaryFeed', @Description = N'PlantSafety dashboard context enriches IncidentMgt summary feed rows (CategoryName/LocationName).';
EXEC [IncidentMgtIntegration].[usp_OutboundEventRegistry_Register]
    @FeedName = N'SafetyCulture_To_RiskFactors', @SourceCapability = N'Safety Culture Index',
    @TargetCapability = N'RiskMatrix.RiskMatrixScore', @Description = N'Safety culture index feeds as a risk factor input to IncidentMgt risk scoring.';
EXEC [IncidentMgtIntegration].[usp_OutboundEventRegistry_Register]
    @FeedName = N'SafetyLeadingIndicators_To_Forecasting', @SourceCapability = N'Safety Leading Indicators',
    @TargetCapability = N'Forecast.SeverityForecast', @Description = N'PlantSafety leading indicators feed the IncidentMgt OLS severity forecasting engine.';
GO
