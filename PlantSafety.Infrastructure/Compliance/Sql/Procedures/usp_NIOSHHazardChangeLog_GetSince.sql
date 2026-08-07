USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Dave Ross
-- Create date:   2026-08-07
-- Procedure:     Compliance.usp_NIOSHHazardChangeLog_GetSince
-- Description:   Retrieves NIOSHHazardChangeLog rows detected at or after a
--                given UTC timestamp (mirrors usp_OSHAStandardChangeLog_GetSince).
-- Notes:
--   - Validates all input parameters
--   - Change log is append-only; no soft-delete filtering needed
--   - Best-effort audit logging for viewer access
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].[usp_NIOSHHazardChangeLog_GetSince]
    @SinceUtc DATETIME2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        ------------------------------------------------------------
        -- Input validation
        ------------------------------------------------------------
        IF @SinceUtc IS NULL
            THROW 60007, 'SinceUtc is required.', 1;

        ------------------------------------------------------------
        -- Audit: viewer access (best-effort, non-fatal)
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'NIOSHHazardChangeLog', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                CONCAT(N'SinceUtc=', CONVERT(NVARCHAR(30), @SinceUtc, 126)),
                N'Viewer access via usp_NIOSHHazardChangeLog_GetSince', N'INFO', N'usp_NIOSHHazardChangeLog_GetSince'
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;

        ------------------------------------------------------------
        -- Main query
        ------------------------------------------------------------
        SELECT
            cl.ChangeLogId,
            cl.HazardId,
            cl.ChemicalName,
            cl.ChangeType,
            cl.PreviousProfileHash,
            cl.NewProfileHash,
            cl.DetectedAtUtc,
            cl.Summary
        FROM [Compliance].[NIOSHHazardChangeLog] AS cl
        WHERE cl.DetectedAtUtc >= @SinceUtc
        ORDER BY cl.DetectedAtUtc ASC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext NVARCHAR(600) = CONCAT(N'SinceUtc=', ISNULL(CONVERT(NVARCHAR(30), @SinceUtc, 126), N'NULL'), N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails NVARCHAR(MAX) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @SourceDatabase NVARCHAR(128) = DB_NAME();
        DECLARE @Module NVARCHAR(128) = OBJECT_NAME(@@PROCID);

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (
                ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                SYSUTCDATETIME(), @ErrorContext, @ErrorDetails, @SourceDatabase, @Module
            );
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'NIOSHHazardChangeLog', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO
