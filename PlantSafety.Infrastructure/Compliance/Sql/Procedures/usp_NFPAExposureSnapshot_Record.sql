-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.NFPAStandards.Dashboards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_NFPAExposureSnapshot_Record.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Inserts one Compliance.NFPAExposureSnapshot row. Called once per module by
--                  GetNfpaSiteExposureQueryHandler/GetNfpaTenantExposureQueryHandler after
--                  computing that module's coverage counts -- this is how exposure trend
--                  history accumulates (see NFPAExposureSnapshot.sql's header). Mirrors
--                  usp_OSHAExposureSnapshot_Record. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_NFPAExposureSnapshot_Record]
    @SnapshotId       UNIQUEIDENTIFIER,
    @TenantId         UNIQUEIDENTIFIER,
    @SiteId           UNIQUEIDENTIFIER = NULL,
    @Module           NVARCHAR(100),
    @TotalHazards     INT,
    @CoveredCount     INT,
    @GapCount         INT,
    @HighGapCount     INT,
    @CriticalGapCount INT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Compliance].[NFPAExposureSnapshot]
            (SnapshotId, TenantId, SiteId, Module, TotalHazards, CoveredCount, GapCount, HighGapCount, CriticalGapCount)
        VALUES
            (@SnapshotId, @TenantId, @SiteId, @Module, @TotalHazards, @CoveredCount, @GapCount, @HighGapCount, @CriticalGapCount);
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'NFPAExposureSnapshot', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'SnapshotId=', @SnapshotId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
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
