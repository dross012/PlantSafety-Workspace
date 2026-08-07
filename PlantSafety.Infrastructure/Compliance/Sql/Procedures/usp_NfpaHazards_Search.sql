-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.NFPAStandards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_NfpaHazards_Search.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Optional keyword/code search over Compliance.NFPA_HazardProfile, paged,
--                  ordered by CodeNumber/SectionNumber. Backs
--                  INfpaHazardsRepository.SearchAsync. Mirrors usp_OSHAStandards_Search.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_NfpaHazards_Search]
    @Keyword    NVARCHAR(200) = NULL,
    @CodeNumber NVARCHAR(20)  = NULL,
    @Skip       INT,
    @Take       INT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT HazardId, CodeNumber, SectionNumber, HazardName, HazardDescription,
               RecommendedControls, PPERecommendations, ElectricalSafetyGuidance,
               HotWorkGuidance, FlammableMaterialsGuidance, SourceUrl, CreatedOn, SectionTextHash
        FROM [Compliance].[NFPA_HazardProfile]
        WHERE (@CodeNumber IS NULL OR CodeNumber = @CodeNumber)
          AND (@Keyword IS NULL
               OR HazardName LIKE N'%' + @Keyword + N'%'
               OR HazardDescription LIKE N'%' + @Keyword + N'%')
        ORDER BY CodeNumber, SectionNumber
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'Keyword=', @Keyword, N'; CodeNumber=', @CodeNumber, N'; User=', SYSTEM_USER);
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
