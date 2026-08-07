-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.NFPAStandards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_NfpaHazards_GetByModule.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Retrieves every Compliance.NFPA_HazardProfile row mapped to @ModuleName via
--                  Compliance.NFPA_HazardModuleMapping. Backs
--                  INfpaHazardsRepository.GetHazardsByModuleAsync. Mirrors
--                  usp_OSHARequirements_GetByModule. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_NfpaHazards_GetByModule]
    @ModuleName NVARCHAR(100)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT h.HazardId, h.CodeNumber, h.SectionNumber, h.HazardName, h.HazardDescription,
               h.RecommendedControls, h.PPERecommendations, h.ElectricalSafetyGuidance,
               h.HotWorkGuidance, h.FlammableMaterialsGuidance, h.SourceUrl, h.CreatedOn,
               h.SectionTextHash, mm.MappingId, mm.MappingRuleCode, mm.Rationale, mm.Severity
        FROM [Compliance].[NFPA_HazardProfile] h
        JOIN [Compliance].[NFPA_HazardModuleMapping] mm ON mm.HazardId = h.HazardId
        WHERE mm.ModuleName = @ModuleName
        ORDER BY h.CodeNumber, h.SectionNumber;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ModuleName=', @ModuleName, N'; User=', SYSTEM_USER);
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
