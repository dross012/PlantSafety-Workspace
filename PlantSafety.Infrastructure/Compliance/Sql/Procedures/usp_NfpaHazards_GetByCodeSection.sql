-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.NFPAStandards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_NfpaHazards_GetByCodeSection.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Compliance.NFPA_HazardProfile row by (CodeNumber,
--                  SectionNumber). Backs INfpaHazardsRepository.GetByCodeSectionAsync.
--                  Mirrors usp_OSHAStandards_GetBySection. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_NfpaHazards_GetByCodeSection]
    @CodeNumber    NVARCHAR(20),
    @SectionNumber NVARCHAR(50)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT HazardId, CodeNumber, SectionNumber, HazardName, HazardDescription,
               RecommendedControls, PPERecommendations, ElectricalSafetyGuidance,
               HotWorkGuidance, FlammableMaterialsGuidance, SourceUrl, CreatedOn, SectionTextHash
        FROM [Compliance].[NFPA_HazardProfile]
        WHERE CodeNumber = @CodeNumber AND SectionNumber = @SectionNumber;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CodeNumber=', @CodeNumber, N'; SectionNumber=', @SectionNumber, N'; User=', SYSTEM_USER);
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
