-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.FMCSAStandards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FmcsaStandards_Search.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Optional keyword/part search over Compliance.FMCSAStandard, paged, ordered by
--                  PartNumber/SectionNumber. Backs IFmcsaStandardsRepository.SearchAsync. Mirrors
--                  usp_OSHAStandards_Search.sql. @PartNumber is nvarchar(10), matching
--                  Compliance.FMCSAStandard.PartNumber (CFR part numbers are stored as text, not
--                  int). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_FmcsaStandards_Search]
    @Keyword    NVARCHAR(200) = NULL,
    @PartNumber NVARCHAR(10)  = NULL,
    @Skip       INT,
    @Take       INT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT StandardId, PartNumber, SubpartLetter, SectionNumber, SectionTitle, SectionText,
               SectionTextHash, SourceUrl, CreatedOn, RowVersion
        FROM [Compliance].[FMCSAStandard]
        WHERE (@PartNumber IS NULL OR PartNumber = @PartNumber)
          AND (@Keyword IS NULL
               OR SectionTitle LIKE N'%' + @Keyword + N'%'
               OR SectionText LIKE N'%' + @Keyword + N'%')
        ORDER BY PartNumber, SectionNumber
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'Keyword=', @Keyword, N'; PartNumber=', @PartNumber, N'; User=', SYSTEM_USER);
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
