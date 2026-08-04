-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability.AdvancedMetrics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_SustainabilityTarget_ListByLedger.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Lists SustainabilityTarget rows for a ledger. Backs aggregate child hydration
--                  and ListSustainabilityTargetsAndProgressQuery.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SustainabilityAdvancedMetrics].[usp_SustainabilityTarget_ListByLedger]
    @AdvancedMetricsLedgerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT SustainabilityTargetId, AdvancedMetricsLedgerId, TargetType, BaselineValue, TargetValue, CurrentValue, TargetDateUtc, Status, SetAtUtc, SetBy, LastProgressUpdateAtUtc
        FROM [SustainabilityAdvancedMetrics].[SustainabilityTarget]
        WHERE AdvancedMetricsLedgerId = @AdvancedMetricsLedgerId AND TenantId = @TenantId
        ORDER BY SetAtUtc;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AdvancedMetricsLedgerId=', @AdvancedMetricsLedgerId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
