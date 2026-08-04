-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors.CognitiveLoadAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_TaskComplexityProfile_Insert.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Inserts a new TaskComplexityProfile child row. Idempotent by design. Backs
--                  DefineTaskComplexityProfileCommandHandler.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_TaskComplexityProfile_Insert]
    @TaskComplexityProfileId UNIQUEIDENTIFIER,
    @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @TaskCode NVARCHAR(50),
    @TaskName NVARCHAR(200),
    @ComplexityScore INT,
    @Description NVARCHAR(1000),
    @DefinedAtUtc DATETIME2(3),
    @DefinedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [HumanFactorsCognitiveLoadAnalytics].[TaskComplexityProfile] WHERE TaskComplexityProfileId = @TaskComplexityProfileId)
        INSERT INTO [HumanFactorsCognitiveLoadAnalytics].[TaskComplexityProfile]
        (TaskComplexityProfileId, CognitiveLoadLedgerId, TenantId, TaskCode, TaskName, ComplexityScore, Description, DefinedAtUtc, DefinedBy)
        VALUES
        (@TaskComplexityProfileId, @CognitiveLoadLedgerId, @TenantId, @TaskCode, @TaskName, @ComplexityScore, @Description, @DefinedAtUtc, @DefinedBy);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TaskComplexityProfileId=', @TaskComplexityProfileId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
