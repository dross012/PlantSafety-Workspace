-- =============================================
-- Enterprise.Integration -- IncidentMgt-Integration
-- Script:  22-usp-reads.sql
-- Purpose: Thin stored-procedure wrappers over the read-only views in 30-views.sql, plus
--          registry/governance-log reads. Required because PlantSafety.Infrastructure's
--          DapperQueryExecutor executes CommandType.StoredProcedure only (see
--          PlantSafety.Infrastructure/Persistence/Dapper/Queries/DapperQueryExecutor.cs) --
--          it cannot SELECT directly from a view. Mirrors EmergencyMgt-Integration's own
--          22-usp-reads.sql pattern exactly.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Backs /plantsafety/incidents/summary
CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_IncidentDashboard_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SourceIncidentId, IncidentNumber, Title, SeverityCode, SeverityRank, StatusCode,
           IsStatusTerminal, CategoryName, LocationName, OccurredUtc, ClosedUtc, ReceivedUtc
    FROM [IncidentMgtIntegration].[vwIncidentDashboardReadOnly]
    ORDER BY OccurredUtc DESC;
END
GO

-- Backs /plantsafety/incidents/trends
CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_IncidentTrend_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OccurredDate, IncidentCount, OpenCount, AvgSeverityRank
    FROM [IncidentMgtIntegration].[vwIncidentTrendWidget]
    ORDER BY OccurredDate DESC;
END
GO

-- Backs /plantsafety/incidents/risk
CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_SeverityDistribution_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SeverityCode, SeverityRank, RiskRating, IncidentCount
    FROM [IncidentMgtIntegration].[vwSeverityDistribution]
    ORDER BY SeverityRank DESC;
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_CorrectiveActionStatus_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SourceIncidentId, IncidentNumber, CorrectiveActionStatus, RootCauseSummary, ReceivedUtc
    FROM [IncidentMgtIntegration].[vwCorrectiveActionStatusReadOnly]
    ORDER BY ReceivedUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_InvestigationStatus_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SourceIncidentId, IncidentNumber, InvestigationStatus, RegulatoryComplianceStatus, ReceivedUtc
    FROM [IncidentMgtIntegration].[vwInvestigationStatusReadOnly]
    ORDER BY ReceivedUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_GovernanceConsumptionLog_GetLatestBySignalType]
    @SignalType NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 GovernanceConsumptionLogId, SignalType, PayloadJson, ConsumedUtc
    FROM [IncidentMgtIntegration].[GovernanceConsumptionLog]
    WHERE SignalType = @SignalType
    ORDER BY ConsumedUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_RoutingKeyRegistry_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoutingKeyRegistryId, RoutingKey, Description, IsActive, RegisteredUtc, LastSeenUtc
    FROM [IncidentMgtIntegration].[RoutingKeyRegistry]
    ORDER BY RoutingKey;
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_OutboundEventRegistry_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OutboundEventRegistryId, FeedName, SourceCapability, TargetCapability, Description,
           IsActive, LastPublishedUtc
    FROM [IncidentMgtIntegration].[OutboundEventRegistry]
    ORDER BY FeedName;
END
GO
