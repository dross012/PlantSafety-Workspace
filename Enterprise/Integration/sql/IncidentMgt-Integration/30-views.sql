-- =============================================
-- Enterprise.Integration -- IncidentMgt-Integration
-- Script:  30-views.sql
-- Purpose: Read-only views backing the PlantSafety UI Integration widgets (Section 3.4) and
--          the PlantSafety API proxy endpoints (Section 3.5: /plantsafety/incidents/summary,
--          /plantsafety/incidents/trends, /plantsafety/incidents/risk). Deterministic,
--          soft-delete-agnostic (source feed has no IsDeleted -- rows are upserted/superseded).
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Read-only incident dashboard (backs /plantsafety/incidents/summary)
CREATE OR ALTER VIEW [IncidentMgtIntegration].[vwIncidentDashboardReadOnly]
AS
    SELECT
        SourceIncidentId, IncidentNumber, Title, SeverityCode, SeverityRank, StatusCode,
        IsStatusTerminal, CategoryName, LocationName, OccurredUtc, ClosedUtc, ReceivedUtc
    FROM [IncidentMgtIntegration].[IncidentSummaryFeed];
GO

-- Read-only incident trend widget (backs /plantsafety/incidents/trends)
CREATE OR ALTER VIEW [IncidentMgtIntegration].[vwIncidentTrendWidget]
AS
    SELECT
        CAST(OccurredUtc AS DATE) AS OccurredDate,
        COUNT(*) AS IncidentCount,
        SUM(CASE WHEN IsStatusTerminal = 0 THEN 1 ELSE 0 END) AS OpenCount,
        AVG(CAST(SeverityRank AS DECIMAL(9,4))) AS AvgSeverityRank
    FROM [IncidentMgtIntegration].[IncidentSummaryFeed]
    GROUP BY CAST(OccurredUtc AS DATE);
GO

-- Read-only severity distribution (backs /plantsafety/incidents/risk)
CREATE OR ALTER VIEW [IncidentMgtIntegration].[vwSeverityDistribution]
AS
    SELECT
        SeverityCode, SeverityRank, RiskRating,
        COUNT(*) AS IncidentCount
    FROM [IncidentMgtIntegration].[IncidentSummaryFeed]
    GROUP BY SeverityCode, SeverityRank, RiskRating;
GO

-- Read-only corrective action status widget
CREATE OR ALTER VIEW [IncidentMgtIntegration].[vwCorrectiveActionStatusReadOnly]
AS
    SELECT
        SourceIncidentId, IncidentNumber, CorrectiveActionStatus, RootCauseSummary, ReceivedUtc
    FROM [IncidentMgtIntegration].[IncidentSummaryFeed]
    WHERE CorrectiveActionStatus IS NOT NULL;
GO

-- Read-only investigation status widget
CREATE OR ALTER VIEW [IncidentMgtIntegration].[vwInvestigationStatusReadOnly]
AS
    SELECT
        SourceIncidentId, IncidentNumber, InvestigationStatus, RegulatoryComplianceStatus, ReceivedUtc
    FROM [IncidentMgtIntegration].[IncidentSummaryFeed]
    WHERE InvestigationStatus IS NOT NULL;
GO
