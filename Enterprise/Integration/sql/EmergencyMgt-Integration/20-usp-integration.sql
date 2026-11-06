-- =============================================
-- Enterprise.Integration -- EmergencyMgt-Integration
-- Script:      20-usp-integration.sql
-- Purpose:     usp_EmergencySummaryInbound_Upsert, usp_EmergencyLifecycleInbound_Record,
--              usp_EmergencyCorrectiveActionInbound_Upsert, usp_EmergencyReadinessInbound_Upsert,
--              usp_RoutingKeyRegistry_Register, usp_RoutingKeyRegistry_Heartbeat,
--              usp_EmergencyGovernanceLog_Record, usp_IntegrationHealth_Check.
-- Idempotent:  CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_EmergencySummaryInbound_Upsert]
    @SourceEmergencyEventId UNIQUEIDENTIFIER,
    @EmergencyTypeName      NVARCHAR(64),
    @Severity                NVARCHAR(32),
    @StatusCode              NVARCHAR(32),
    @IsStatusTerminal        BIT,
    @Location                NVARCHAR(150) = NULL,
    @InitiatedBy             NVARCHAR(128) = NULL,
    @StartedUtc              DATETIME2,
    @EndedUtc                DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SourceEmergencyEventId IS NULL THROW 50001, N'SourceEmergencyEventId is required.', 1;

        MERGE [EmergencyMgtIntegration].[EmergencySummaryInbound] AS target
        USING (SELECT @SourceEmergencyEventId AS SourceEmergencyEventId) AS src
            ON target.[SourceEmergencyEventId] = src.SourceEmergencyEventId
        WHEN MATCHED THEN
            UPDATE SET
                [EmergencyTypeName] = @EmergencyTypeName, [Severity] = @Severity, [StatusCode] = @StatusCode,
                [IsStatusTerminal] = @IsStatusTerminal, [Location] = @Location, [InitiatedBy] = @InitiatedBy,
                [StartedUtc] = @StartedUtc, [EndedUtc] = @EndedUtc, [ReceivedUtc] = SYSUTCDATETIME()
        WHEN NOT MATCHED THEN
            INSERT ([SourceEmergencyEventId], [EmergencyTypeName], [Severity], [StatusCode], [IsStatusTerminal],
                    [Location], [InitiatedBy], [StartedUtc], [EndedUtc])
            VALUES (@SourceEmergencyEventId, @EmergencyTypeName, @Severity, @StatusCode, @IsStatusTerminal,
                    @Location, @InitiatedBy, @StartedUtc, @EndedUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextG0 NVARCHAR(200) = CONCAT(N'Params=SourceEmergencyEventId=', @SourceEmergencyEventId, N'; User=', SYSTEM_USER);
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

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_EmergencyLifecycleInbound_Record]
    @SourceEmergencyEventId UNIQUEIDENTIFIER,
    @FromStatus              NVARCHAR(32) = NULL,
    @ToStatus                NVARCHAR(32),
    @TransitionUtc           DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SourceEmergencyEventId IS NULL THROW 50001, N'SourceEmergencyEventId is required.', 1;

        IF NOT EXISTS (
            SELECT 1 FROM [EmergencyMgtIntegration].[EmergencyLifecycleInbound]
            WHERE [SourceEmergencyEventId] = @SourceEmergencyEventId AND [ToStatus] = @ToStatus AND [TransitionUtc] = @TransitionUtc
        )
        BEGIN
            INSERT INTO [EmergencyMgtIntegration].[EmergencyLifecycleInbound]
                ([SourceEmergencyEventId], [FromStatus], [ToStatus], [TransitionUtc])
            VALUES
                (@SourceEmergencyEventId, @FromStatus, @ToStatus, @TransitionUtc);
        END
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextG1 NVARCHAR(200) = CONCAT(N'Params=SourceEmergencyEventId=', @SourceEmergencyEventId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsG1 NVARCHAR(MAX) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextG1, @ErrorDetailsG1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_EmergencyCorrectiveActionInbound_Upsert]
    @SourceCorrectiveActionId UNIQUEIDENTIFIER,
    @SourceEmergencyEventId   UNIQUEIDENTIFIER = NULL,
    @Description               NVARCHAR(MAX),
    @AssignedTo                NVARCHAR(128),
    @DueUtc                    DATETIME2 = NULL,
    @StatusCode                NVARCHAR(32)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SourceCorrectiveActionId IS NULL THROW 50001, N'SourceCorrectiveActionId is required.', 1;

        MERGE [EmergencyMgtIntegration].[EmergencyCorrectiveActionInbound] AS target
        USING (SELECT @SourceCorrectiveActionId AS SourceCorrectiveActionId) AS src
            ON target.[SourceCorrectiveActionId] = src.SourceCorrectiveActionId
        WHEN MATCHED THEN
            UPDATE SET
                [SourceEmergencyEventId] = @SourceEmergencyEventId, [Description] = @Description,
                [AssignedTo] = @AssignedTo, [DueUtc] = @DueUtc, [StatusCode] = @StatusCode, [ReceivedUtc] = SYSUTCDATETIME()
        WHEN NOT MATCHED THEN
            INSERT ([SourceCorrectiveActionId], [SourceEmergencyEventId], [Description], [AssignedTo], [DueUtc], [StatusCode])
            VALUES (@SourceCorrectiveActionId, @SourceEmergencyEventId, @Description, @AssignedTo, @DueUtc, @StatusCode);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextG2 NVARCHAR(200) = CONCAT(N'Params=SourceCorrectiveActionId=', @SourceCorrectiveActionId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsG2 NVARCHAR(MAX) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextG2, @ErrorDetailsG2, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_EmergencyReadinessInbound_Upsert]
    @SourceReadinessScoreId UNIQUEIDENTIFIER,
    @OverallScore             DECIMAL(5,2),
    @FactorScoresJson         NVARCHAR(MAX),
    @ComputedUtc              DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SourceReadinessScoreId IS NULL THROW 50001, N'SourceReadinessScoreId is required.', 1;
        IF ISJSON(@FactorScoresJson) = 0 THROW 50001, N'FactorScoresJson must be valid JSON.', 1;

        MERGE [EmergencyMgtIntegration].[EmergencyReadinessInbound] AS target
        USING (SELECT @SourceReadinessScoreId AS SourceReadinessScoreId) AS src
            ON target.[SourceReadinessScoreId] = src.SourceReadinessScoreId
        WHEN MATCHED THEN
            UPDATE SET [OverallScore] = @OverallScore, [FactorScoresJson] = @FactorScoresJson,
                       [ComputedUtc] = @ComputedUtc, [ReceivedUtc] = SYSUTCDATETIME()
        WHEN NOT MATCHED THEN
            INSERT ([SourceReadinessScoreId], [OverallScore], [FactorScoresJson], [ComputedUtc])
            VALUES (@SourceReadinessScoreId, @OverallScore, @FactorScoresJson, @ComputedUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextG3 NVARCHAR(200) = CONCAT(N'Params=SourceReadinessScoreId=', @SourceReadinessScoreId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsG3 NVARCHAR(MAX) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextG3, @ErrorDetailsG3, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register]
    @RoutingKey  NVARCHAR(100),
    @Description NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RoutingKey IS NULL THROW 50001, N'RoutingKey is required.', 1;

        MERGE [EmergencyMgtIntegration].[RoutingKeyRegistry] AS target
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

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Heartbeat]
    @RoutingKey NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RoutingKey IS NULL THROW 50001, N'RoutingKey is required.', 1;
        UPDATE [EmergencyMgtIntegration].[RoutingKeyRegistry]
        SET [LastSeenUtc] = SYSUTCDATETIME()
        WHERE [RoutingKey] = @RoutingKey;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_EmergencyGovernanceLog_Record]
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

        INSERT INTO [EmergencyMgtIntegration].[EmergencyGovernanceLog] ([SignalType], [PayloadJson])
        VALUES (@SignalType, @PayloadJson);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- Installer/health-monitoring check: backs /integration/emergency/health.
CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_IntegrationHealth_Check]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        SELECT
            (SELECT COUNT(*) FROM [EmergencyMgtIntegration].[RoutingKeyRegistry] WHERE [IsActive] = 1) AS ActiveRoutingKeyCount,
            (SELECT COUNT(*) FROM [EmergencyMgtIntegration].[RoutingKeyRegistry] WHERE [IsActive] = 1 AND ([LastSeenUtc] IS NULL OR [LastSeenUtc] < DATEADD(HOUR, -24, SYSUTCDATETIME()))) AS StaleRoutingKeyCount,
            (SELECT MAX([ReceivedUtc]) FROM [EmergencyMgtIntegration].[EmergencySummaryInbound]) AS LastSummaryReceivedUtc,
            (SELECT MAX([ReceivedUtc]) FROM [EmergencyMgtIntegration].[EmergencyLifecycleInbound]) AS LastLifecycleReceivedUtc,
            (SELECT MAX([ReceivedUtc]) FROM [EmergencyMgtIntegration].[EmergencyCorrectiveActionInbound]) AS LastCorrectiveActionReceivedUtc,
            (SELECT MAX([ReceivedUtc]) FROM [EmergencyMgtIntegration].[EmergencyReadinessInbound]) AS LastReadinessReceivedUtc,
            (SELECT MAX([ConsumedUtc]) FROM [EmergencyMgtIntegration].[EmergencyGovernanceLog] WHERE [SignalType] = N'Health') AS LastHealthSignalUtc,
            (SELECT MAX([ConsumedUtc]) FROM [EmergencyMgtIntegration].[EmergencyGovernanceLog] WHERE [SignalType] = N'DriftCheck') AS LastDriftCheckUtc,
            (SELECT MAX([ConsumedUtc]) FROM [EmergencyMgtIntegration].[EmergencyGovernanceLog] WHERE [SignalType] = N'VersionManifest') AS LastVersionManifestUtc;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
