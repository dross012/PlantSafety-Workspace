-- =============================================
-- Enterprise.Integration -- EmergencyMgt-Integration
-- Script:  22-usp-reads.sql
-- Purpose: Thin stored-procedure wrappers over the read-only views in 30-views.sql, plus
--          registry/governance-log reads. Required because PlantSafety.Infrastructure's
--          DapperQueryExecutor executes CommandType.StoredProcedure only (see
--          PlantSafety.Infrastructure/Persistence/Dapper/Queries/DapperQueryExecutor.cs) --
--          it cannot SELECT directly from a view.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_EmergencySummary_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SourceEmergencyEventId, EmergencyTypeName, Severity, StatusCode, IsStatusTerminal,
           Location, InitiatedBy, StartedUtc, EndedUtc, ReceivedUtc
    FROM [EmergencyMgtIntegration].[vwEmergencySummary]
    ORDER BY StartedUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_EmergencyLifecycle_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SourceEmergencyEventId, FromStatus, ToStatus, TransitionUtc, ReceivedUtc
    FROM [EmergencyMgtIntegration].[vwEmergencyLifecycle]
    ORDER BY TransitionUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_EmergencyCorrectiveActions_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SourceCorrectiveActionId, SourceEmergencyEventId, Description, AssignedTo, DueUtc,
           StatusCode, ReceivedUtc
    FROM [EmergencyMgtIntegration].[vwEmergencyCorrectiveActions]
    ORDER BY ReceivedUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_EmergencyReadiness_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SourceReadinessScoreId, OverallScore, FactorScoresJson, ComputedUtc, ReceivedUtc
    FROM [EmergencyMgtIntegration].[vwEmergencyReadiness]
    ORDER BY ComputedUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_EmergencyKpis_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT EmergencyTypeName, EventCount, OpenCount, ClosedCount,
           LatestOverallReadinessScore, LatestReadinessComputedUtc
    FROM [EmergencyMgtIntegration].[vwEmergencyKPIs]
    ORDER BY EmergencyTypeName;
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_EmergencyGovernanceLog_GetLatestBySignalType]
    @SignalType NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 EmergencyGovernanceLogId, SignalType, PayloadJson, ConsumedUtc
    FROM [EmergencyMgtIntegration].[EmergencyGovernanceLog]
    WHERE SignalType = @SignalType
    ORDER BY ConsumedUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoutingKeyRegistryId, RoutingKey, Description, IsActive, RegisteredUtc, LastSeenUtc
    FROM [EmergencyMgtIntegration].[RoutingKeyRegistry]
    ORDER BY RoutingKey;
END
GO
