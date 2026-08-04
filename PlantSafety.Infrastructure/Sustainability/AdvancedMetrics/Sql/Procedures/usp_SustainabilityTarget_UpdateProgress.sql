-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability.AdvancedMetrics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_SustainabilityTarget_UpdateProgress.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Updates a SustainabilityTarget's tracked current value and status. Backs
--                  AdvancedMetricsLedger.UpdateTargetProgress persistence.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SustainabilityAdvancedMetrics].[usp_SustainabilityTarget_UpdateProgress]
    @SustainabilityTargetId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @CurrentValue DECIMAL(18,6),
    @Status NVARCHAR(20),
    @UpdatedAtUtc DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [SustainabilityAdvancedMetrics].[SustainabilityTarget]
        SET CurrentValue = @CurrentValue, Status = @Status, LastProgressUpdateAtUtc = @UpdatedAtUtc
        WHERE SustainabilityTargetId = @SustainabilityTargetId AND TenantId = @TenantId;

        SELECT @@ROWCOUNT;
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
