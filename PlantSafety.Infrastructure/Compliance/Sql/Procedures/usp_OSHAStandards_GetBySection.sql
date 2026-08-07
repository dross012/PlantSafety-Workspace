-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.OSHAStandards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_OSHAStandards_GetBySection.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Compliance.OSHAStandard row by (PartNumber, Section).
--                  Backs IOSHAStandardsRepository.GetBySectionAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_OSHAStandards_GetBySection]
    @PartNumber INT,
    @Section    NVARCHAR(50)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT OSHAStandardId, TitleNumber, PartNumber, PartTitle, Subpart, SubpartTitle,
               Section, SectionTitle, SectionText, EffectiveDate, Status, SourceUrl,
               CreatedAtUtc, UpdatedAtUtc
        FROM [Compliance].[OSHAStandard]
        WHERE PartNumber = @PartNumber AND Section = @Section;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PartNumber=', @PartNumber, N'; Section=', @Section, N'; User=', SYSTEM_USER);
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
