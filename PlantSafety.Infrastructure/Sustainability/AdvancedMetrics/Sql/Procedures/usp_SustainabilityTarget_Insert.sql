-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability.AdvancedMetrics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_SustainabilityTarget_Insert.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Inserts a new SustainabilityTarget child row.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SustainabilityAdvancedMetrics].[usp_SustainabilityTarget_Insert]
    @SustainabilityTargetId UNIQUEIDENTIFIER,
    @AdvancedMetricsLedgerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @TargetType NVARCHAR(50),
    @BaselineValue DECIMAL(18,6),
    @TargetValue DECIMAL(18,6),
    @CurrentValue DECIMAL(18,6),
    @TargetDateUtc DATETIME2(3),
    @Status NVARCHAR(20),
    @SetAtUtc DATETIME2(3),
    @SetBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        -- Idempotent by design: see usp_CarbonEmissionRecord_Insert remarks. Progress updates to
        -- an already-persisted target go through usp_SustainabilityTarget_UpdateProgress instead.
        IF NOT EXISTS (SELECT 1 FROM [SustainabilityAdvancedMetrics].[SustainabilityTarget] WHERE SustainabilityTargetId = @SustainabilityTargetId)
        INSERT INTO [SustainabilityAdvancedMetrics].[SustainabilityTarget]
        (SustainabilityTargetId, AdvancedMetricsLedgerId, TenantId, TargetType, BaselineValue, TargetValue, CurrentValue, TargetDateUtc, Status, SetAtUtc, SetBy)
        VALUES
        (@SustainabilityTargetId, @AdvancedMetricsLedgerId, @TenantId, @TargetType, @BaselineValue, @TargetValue, @CurrentValue, @TargetDateUtc, @Status, @SetAtUtc, @SetBy);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SustainabilityTargetId=', @SustainabilityTargetId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
