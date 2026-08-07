-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.FMCSAStandards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FmcsaStandards_GetBySection.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Compliance.FMCSAStandard row by (PartNumber, SectionNumber).
--                  PartNumber is NVARCHAR(10) to match Compliance.FMCSAStandard.PartNumber
--                  (CFR part numbers are stored as text, not int). Backs
--                  IFmcsaStandardsRepository.GetBySectionAsync. Mirrors
--                  usp_OSHAStandards_GetBySection.sql. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_FmcsaStandards_GetBySection]
    @PartNumber    NVARCHAR(10),
    @SectionNumber NVARCHAR(50)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT StandardId, PartNumber, SubpartLetter, SectionNumber, SectionTitle, SectionText,
               SectionTextHash, SourceUrl, CreatedOn, RowVersion
        FROM [Compliance].[FMCSAStandard]
        WHERE PartNumber = @PartNumber AND SectionNumber = @SectionNumber;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PartNumber=', @PartNumber, N'; SectionNumber=', @SectionNumber, N'; User=', SYSTEM_USER);
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
