-- =============================================
-- Enterprise.Integration -- IncidentMgt-Integration
-- Script:      20-usp-integration.sql
-- Purpose:     usp_IncidentSummaryFeed_Upsert, usp_RoutingKeyRegistry_Register,
--              usp_RoutingKeyRegistry_Heartbeat, usp_GovernanceConsumption_Record.
-- Idempotent:  CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_IncidentSummaryFeed_Upsert]
    @SourceIncidentId       UNIQUEIDENTIFIER,
    @IncidentNumber         NVARCHAR(30),
    @Title                  NVARCHAR(200),
    @SeverityCode           NVARCHAR(50),
    @SeverityRank           INT,
    @StatusCode             NVARCHAR(30),
    @IsStatusTerminal       BIT,
    @CategoryName           NVARCHAR(100) = NULL,
    @LocationName           NVARCHAR(150) = NULL,
    @OccurredUtc            DATETIME2,
    @ClosedUtc              DATETIME2 = NULL,
    @RiskRating             NVARCHAR(30) = NULL,
    @InvestigationStatus    NVARCHAR(30) = NULL,
    @RootCauseSummary       NVARCHAR(500) = NULL,
    @CorrectiveActionStatus NVARCHAR(30) = NULL,
    @RegulatoryComplianceStatus NVARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SourceIncidentId IS NULL THROW 50001, N'SourceIncidentId is required.', 1;

        MERGE [IncidentMgtIntegration].[IncidentSummaryFeed] AS target
        USING (SELECT @SourceIncidentId AS SourceIncidentId) AS src
            ON target.[SourceIncidentId] = src.SourceIncidentId
        WHEN MATCHED THEN
            UPDATE SET
                [IncidentNumber] = @IncidentNumber, [Title] = @Title, [SeverityCode] = @SeverityCode,
                [SeverityRank] = @SeverityRank, [StatusCode] = @StatusCode, [IsStatusTerminal] = @IsStatusTerminal,
                [CategoryName] = @CategoryName, [LocationName] = @LocationName, [OccurredUtc] = @OccurredUtc,
                [ClosedUtc] = @ClosedUtc, [RiskRating] = @RiskRating, [InvestigationStatus] = @InvestigationStatus,
                [RootCauseSummary] = @RootCauseSummary, [CorrectiveActionStatus] = @CorrectiveActionStatus,
                [RegulatoryComplianceStatus] = @RegulatoryComplianceStatus, [ReceivedUtc] = SYSUTCDATETIME()
        WHEN NOT MATCHED THEN
            INSERT ([SourceIncidentId], [IncidentNumber], [Title], [SeverityCode], [SeverityRank], [StatusCode],
                    [IsStatusTerminal], [CategoryName], [LocationName], [OccurredUtc], [ClosedUtc], [RiskRating],
                    [InvestigationStatus], [RootCauseSummary], [CorrectiveActionStatus], [RegulatoryComplianceStatus])
            VALUES (@SourceIncidentId, @IncidentNumber, @Title, @SeverityCode, @SeverityRank, @StatusCode,
                    @IsStatusTerminal, @CategoryName, @LocationName, @OccurredUtc, @ClosedUtc, @RiskRating,
                    @InvestigationStatus, @RootCauseSummary, @CorrectiveActionStatus, @RegulatoryComplianceStatus);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextG0 NVARCHAR(200) = CONCAT(N'Params=SourceIncidentId=', @SourceIncidentId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsG0 NVARCHAR(MAX) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextG0, @ErrorDetailsG0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_RoutingKeyRegistry_Register]
    @RoutingKey  NVARCHAR(100),
    @Description NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RoutingKey IS NULL THROW 50001, N'RoutingKey is required.', 1;

        MERGE [IncidentMgtIntegration].[RoutingKeyRegistry] AS target
        USING (SELECT @RoutingKey AS RoutingKey) AS src
            ON target.[RoutingKey] = src.RoutingKey
        WHEN MATCHED THEN
            UPDATE SET [Description] = ISNULL(@Description, target.[Description]), [IsActive] = 1
        WHEN NOT MATCHED THEN
            INSERT ([RoutingKey], [Description]) VALUES (@RoutingKey, @Description);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_RoutingKeyRegistry_Heartbeat]
    @RoutingKey NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RoutingKey IS NULL THROW 50001, N'RoutingKey is required.', 1;
        UPDATE [IncidentMgtIntegration].[RoutingKeyRegistry]
        SET [LastSeenUtc] = SYSUTCDATETIME()
        WHERE [RoutingKey] = @RoutingKey;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_GovernanceConsumption_Record]
    @SignalType   NVARCHAR(30),
    @PayloadJson  NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SignalType NOT IN (N'VersionManifest', N'DriftCheck', N'Health')
            THROW 50001, N'SignalType must be VersionManifest, DriftCheck, or Health.', 1;
        IF ISJSON(@PayloadJson) = 0
            THROW 50001, N'PayloadJson must be valid JSON.', 1;

        INSERT INTO [IncidentMgtIntegration].[GovernanceConsumptionLog] ([SignalType], [PayloadJson])
        VALUES (@SignalType, @PayloadJson);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- Installer health check: used by the PlantSafety installer integration checklist
-- ("Add IncidentMgt integration checks", "Add IncidentMgt routing checks",
-- "Add IncidentMgt health endpoint checks" -- Section 1.10 / Section 3.7).
CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_IntegrationHealth_Check]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        SELECT
            (SELECT COUNT(*) FROM [IncidentMgtIntegration].[RoutingKeyRegistry] WHERE [IsActive] = 1) AS ActiveRoutingKeyCount,
            (SELECT COUNT(*) FROM [IncidentMgtIntegration].[RoutingKeyRegistry] WHERE [IsActive] = 1 AND ([LastSeenUtc] IS NULL OR [LastSeenUtc] < DATEADD(HOUR, -24, SYSUTCDATETIME()))) AS StaleRoutingKeyCount,
            (SELECT MAX([ReceivedUtc]) FROM [IncidentMgtIntegration].[IncidentSummaryFeed]) AS LastSummaryFeedReceivedUtc,
            (SELECT MAX([ConsumedUtc]) FROM [IncidentMgtIntegration].[GovernanceConsumptionLog] WHERE [SignalType] = N'Health') AS LastHealthSignalUtc,
            (SELECT MAX([ConsumedUtc]) FROM [IncidentMgtIntegration].[GovernanceConsumptionLog] WHERE [SignalType] = N'DriftCheck') AS LastDriftCheckUtc,
            (SELECT MAX([ConsumedUtc]) FROM [IncidentMgtIntegration].[GovernanceConsumptionLog] WHERE [SignalType] = N'VersionManifest') AS LastVersionManifestUtc;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
