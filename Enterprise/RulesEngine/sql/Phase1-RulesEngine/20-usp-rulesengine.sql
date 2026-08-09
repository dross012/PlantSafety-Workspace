-- =============================================
-- Enterprise.RulesEngine -- Phase1-RulesEngine
-- Script:        20-usp-rulesengine.sql
-- Purpose:       usp_Rule_Upsert, usp_Rule_GetEnabledByType,
--                usp_RuleEvaluationLog_Record.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [EnterpriseRules].[usp_Rule_Upsert]
    @RuleId      uniqueidentifier,
    @Name        nvarchar(200),
    @RuleType    nvarchar(20),
    @IsEnabled   bit = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RuleId IS NULL OR @RuleId = '00000000-0000-0000-0000-000000000000'
            THROW 50001, N'RuleId is required and must not be empty.', 1;

        MERGE [EnterpriseRules].[Rule] AS target
        USING (SELECT @RuleId AS RuleId) AS source ON target.[RuleId] = source.RuleId
        WHEN MATCHED THEN
            UPDATE SET [Name] = @Name, [RuleType] = @RuleType, [IsEnabled] = @IsEnabled,
                       [UpdatedUtc] = SYSUTCDATETIME(), [UpdatedBy] = SYSTEM_USER
        WHEN NOT MATCHED THEN
            INSERT ([RuleId], [Name], [RuleType], [IsEnabled]) VALUES (@RuleId, @Name, @RuleType, @IsEnabled);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextB0 nvarchar(200) = CONCAT(N'Params=RuleId=', @RuleId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsB0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextB0, @ErrorDetailsB0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EnterpriseRules].[usp_Rule_GetEnabledByType]
    @RuleType nvarchar(20)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        SELECT r.[RuleId], r.[Name], r.[RuleType],
               c.[FactKey], c.[Operator], c.[ComparisonValue],
               a.[ActionCode], a.[Description]
        FROM [EnterpriseRules].[Rule] AS r
        LEFT JOIN [EnterpriseRules].[RuleCondition] AS c ON c.[RuleId] = r.[RuleId]
        LEFT JOIN [EnterpriseRules].[RuleAction] AS a ON a.[RuleId] = r.[RuleId]
        WHERE r.[RuleType] = @RuleType AND r.[IsEnabled] = 1 AND ISNULL(r.[IsDeleted], 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextB1 nvarchar(200) = CONCAT(N'Params=RuleType=', @RuleType, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsB1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextB1, @ErrorDetailsB1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EnterpriseRules].[usp_RuleEvaluationLog_Record]
    @RuleId                uniqueidentifier,
    @RuleName               nvarchar(200),
    @Matched                bit,
    @TriggeredActionsJson   nvarchar(max),
    @EvaluatedAtUtc         datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RuleId IS NULL
            THROW 50001, N'RuleId is required.', 1;
        IF ISJSON(@TriggeredActionsJson) = 0
            THROW 50001, N'TriggeredActionsJson must be valid JSON.', 1;

        INSERT INTO [EnterpriseRules].[RuleEvaluationLog] ([RuleId], [RuleName], [Matched], [TriggeredActionsJson], [EvaluatedAtUtc])
        VALUES (@RuleId, @RuleName, @Matched, @TriggeredActionsJson, @EvaluatedAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextB2 nvarchar(200) = CONCAT(N'Params=RuleId=', @RuleId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsB2 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextB2, @ErrorDetailsB2, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
