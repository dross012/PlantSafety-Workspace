-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Standards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Standards_GetEquipmentRequirements.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Retrieves Compliance.StandardsEquipmentRequirement rows, optionally
--                  filtered by EquipmentType and/or RequirementId (both optional, combinable).
--                  Global regulatory-adjacent reference data. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_Standards_GetEquipmentRequirements]
    @EquipmentType NVARCHAR(50)     = NULL,
    @RequirementId UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT EquipmentRequirementId, RequirementId, EquipmentType, RequirementText,
               InspectionIntervalDescription, ControlSystemCategory, CreatedOn, RowVersion
        FROM [Compliance].[StandardsEquipmentRequirement]
        WHERE (@EquipmentType IS NULL OR EquipmentType = @EquipmentType)
          AND (@RequirementId IS NULL OR RequirementId = @RequirementId)
        ORDER BY EquipmentType, RequirementText;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EquipmentType=', @EquipmentType, N'; RequirementId=', @RequirementId, N'; User=', SYSTEM_USER);
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
