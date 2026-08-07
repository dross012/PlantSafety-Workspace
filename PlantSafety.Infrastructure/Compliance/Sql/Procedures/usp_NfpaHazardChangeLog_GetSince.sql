-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.NFPAStandards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_NfpaHazardChangeLog_GetSince.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Retrieves Compliance.NFPA_HazardChangeLog entries detected at or after
--                  @SinceUtc, most recent first. Backs
--                  INfpaHazardsRepository.GetChangesSinceAsync. Mirrors
--                  usp_OSHAStandardChangeLog_GetSince. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_NfpaHazardChangeLog_GetSince]
    @SinceUtc DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT ChangeLogId, HazardId, CodeNumber, SectionNumber, ChangeType,
               PreviousTextHash, NewTextHash, DetectedAtUtc, Summary
        FROM [Compliance].[NFPA_HazardChangeLog]
        WHERE DetectedAtUtc >= @SinceUtc
        ORDER BY DetectedAtUtc DESC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SinceUtc=', @SinceUtc, N'; User=', SYSTEM_USER);
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
