-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors.CognitiveLoadAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CognitiveLoadLedger_GetById.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single CognitiveLoadLedger header row by identity, scoped to tenant.
--                  Backs ICognitiveLoadLedgerRepository.GetByIdAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadLedger_GetById]
    @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT CognitiveLoadLedgerId, TenantId, SiteId, ShiftType, ShiftDateUtc, Status, CreatedAtUtc, CreatedBy
        FROM [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger]
        WHERE CognitiveLoadLedgerId = @CognitiveLoadLedgerId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CognitiveLoadLedgerId=', @CognitiveLoadLedgerId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
