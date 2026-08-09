-- =============================================
-- AI.Reasoning -- Phase1-ReasoningLog
-- Script:        20-usp-reasoningsessionlog.sql
-- Purpose:       usp_ReasoningSessionLog_Record / GetBySessionId.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [AIReasoning].[usp_ReasoningSessionLog_Record]
    @ReasoningSessionId                uniqueidentifier,
    @InputSeverityCode                  nvarchar(30),
    @DigitalTwinSimulatedSeverityCode   nvarchar(30),
    @OverallSeverityCode                nvarchar(30),
    @ConclusionsJson                    nvarchar(max),
    @EvaluatedAtUtc                     datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ReasoningSessionId IS NULL
            THROW 50001, N'ReasoningSessionId is required.', 1;
        IF ISJSON(@ConclusionsJson) = 0
            THROW 50001, N'ConclusionsJson must be valid JSON.', 1;

        INSERT INTO [AIReasoning].[ReasoningSessionLog] (
            [ReasoningSessionId], [InputSeverityCode], [DigitalTwinSimulatedSeverityCode],
            [OverallSeverityCode], [ConclusionsJson], [EvaluatedAtUtc]
        )
        VALUES (
            @ReasoningSessionId, @InputSeverityCode, @DigitalTwinSimulatedSeverityCode,
            @OverallSeverityCode, @ConclusionsJson, @EvaluatedAtUtc
        );
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextG0 nvarchar(200) = CONCAT(N'Params=ReasoningSessionId=', @ReasoningSessionId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsG0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextG0, @ErrorDetailsG0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [AIReasoning].[usp_ReasoningSessionLog_GetBySessionId]
    @ReasoningSessionId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ReasoningSessionId IS NULL
            THROW 50001, N'ReasoningSessionId is required.', 1;

        SELECT [ReasoningSessionLogId], [ReasoningSessionId], [InputSeverityCode],
               [DigitalTwinSimulatedSeverityCode], [OverallSeverityCode], [ConclusionsJson], [EvaluatedAtUtc]
        FROM [AIReasoning].[ReasoningSessionLog]
        WHERE [ReasoningSessionId] = @ReasoningSessionId AND ISNULL([IsDeleted], 0) = 0
        ORDER BY [EvaluatedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextG1 nvarchar(200) = CONCAT(N'Params=ReasoningSessionId=', @ReasoningSessionId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsG1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextG1, @ErrorDetailsG1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
