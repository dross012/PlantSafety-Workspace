-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.PredictiveAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PredictiveAnalytics_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new PredictiveAnalytics aggregate root row into the existing
--                  [Analytics].[PredictiveAnalytics] table (additively extended, see
--                  PredictiveAnalytics.sql). Backs IPredictiveAnalyticsRepository.AddAsync.
--                  TRY/CATCH with AuditDB/ErrorDB logging per the canonical write-procedure
--                  template (mirrors usp_Permits_Insert.sql). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Analytics].[usp_PredictiveAnalytics_Insert]
    @PredictionId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @ModelName NVARCHAR(200),
    @PredictionType NVARCHAR(200),
    @TargetEntityId UNIQUEIDENTIFIER = NULL,
    @Score DECIMAL(18,4),
    @ConfidenceLevel DECIMAL(18,4),
    @PredictionHorizonDays INT,
    @GeneratedAtUtc DATETIME2(3),
    @GeneratedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Analytics].[PredictiveAnalytics]
        (
            PredictionId, TenantId, SiteId, Status, ModelName, PredictionType, TargetEntityId, Score, ConfidenceLevel, PredictionHorizonDays, GeneratedAtUtc, GeneratedBy, IsDeleted
        )
        VALUES
        (
            @PredictionId, @TenantId, @SiteId, @Status, @ModelName, @PredictionType, @TargetEntityId, @Score, @ConfidenceLevel, @PredictionHorizonDays, @GeneratedAtUtc, @GeneratedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PredictiveAnalytics', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PredictionId=', @PredictionId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PredictionId=', @PredictionId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
