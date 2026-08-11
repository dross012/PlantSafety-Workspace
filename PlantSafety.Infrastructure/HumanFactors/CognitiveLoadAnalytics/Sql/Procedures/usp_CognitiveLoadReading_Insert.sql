-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors.CognitiveLoadAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CognitiveLoadReading_Insert.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Inserts a new CognitiveLoadReading child row. Idempotent by design -- the
--                  repository re-submits the aggregate's full in-memory child collection on every
--                  Add/Update call. Backs RecordCognitiveLoadReadingCommandHandler.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadReading_Insert]
    @CognitiveLoadReadingId UNIQUEIDENTIFIER,
    @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @WorkerUserId UNIQUEIDENTIFIER,
    @TaskComplexityProfileId UNIQUEIDENTIFIER = NULL,
    @CognitiveLoadScore DECIMAL(5,2),
    @WorkloadLevel NVARCHAR(20),
    @MeasurementMethod NVARCHAR(100),
    @MeasuredAtUtc DATETIME2(3),
    @RecordedAtUtc DATETIME2(3),
    @RecordedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading] WHERE CognitiveLoadReadingId = @CognitiveLoadReadingId)
        INSERT INTO [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading]
        (CognitiveLoadReadingId, CognitiveLoadLedgerId, TenantId, WorkerUserId, TaskComplexityProfileId, CognitiveLoadScore, WorkloadLevel, MeasurementMethod, MeasuredAtUtc, RecordedAtUtc, RecordedBy)
        VALUES
        (@CognitiveLoadReadingId, @CognitiveLoadLedgerId, @TenantId, @WorkerUserId, @TaskComplexityProfileId, @CognitiveLoadScore, @WorkloadLevel, @MeasurementMethod, @MeasuredAtUtc, @RecordedAtUtc, @RecordedBy);
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'CognitiveLoadReading', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CognitiveLoadReadingId=', @CognitiveLoadReadingId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CognitiveLoadReadingId=', @CognitiveLoadReadingId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
