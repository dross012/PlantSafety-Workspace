-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.OSHAStandards.Dashboards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_OSHAExposureSnapshot_Record.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Inserts one Compliance.OSHAExposureSnapshot row. Called once per module by
--                  GetOshaSiteExposureQueryHandler/GetOshaTenantExposureQueryHandler after
--                  computing that module's coverage counts -- this is how exposure trend history
--                  accumulates (see OSHAExposureSnapshot.sql's header). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_OSHAExposureSnapshot_Record]
    @SnapshotId        UNIQUEIDENTIFIER,
    @TenantId          UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER = NULL,
    @Module            NVARCHAR(100),
    @TotalRequirements INT,
    @CoveredCount      INT,
    @GapCount          INT,
    @HighGapCount      INT,
    @CriticalGapCount  INT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Compliance].[OSHAExposureSnapshot]
            (SnapshotId, TenantId, SiteId, Module, TotalRequirements, CoveredCount, GapCount, HighGapCount, CriticalGapCount)
        VALUES
            (@SnapshotId, @TenantId, @SiteId, @Module, @TotalRequirements, @CoveredCount, @GapCount, @HighGapCount, @CriticalGapCount);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; Module=', @Module, N'; User=', SYSTEM_USER);
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
