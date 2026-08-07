-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.FMCSAStandards.Dashboards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FmcsaDashboard_GetModuleTotals.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Total/High/Critical requirement counts per module -- global reference data, no
--                  tenant/site scoping. Backs IFmcsaAuditGapRepository.GetModuleTotalsAsync.
--                  Mirrors usp_OshaDashboard_GetModuleTotals.sql. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_FmcsaDashboard_GetModuleTotals]
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            AppliesToModule AS Module,
            COUNT(*) AS TotalRequirements,
            SUM(CASE WHEN Severity = 'High' THEN 1 ELSE 0 END) AS HighCount,
            SUM(CASE WHEN Severity = 'Critical' THEN 1 ELSE 0 END) AS CriticalCount
        FROM [Compliance].[FMCSARequirement]
        WHERE AppliesToModule IS NOT NULL
        GROUP BY AppliesToModule
        ORDER BY TotalRequirements DESC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
