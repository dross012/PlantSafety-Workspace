-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RulesEngine
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ComplianceEvaluation_GetHistoryForTarget.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves the ComplianceEvaluation history for a target entity, most recent
--                  first. Backs IComplianceEvaluationRepository.GetHistoryForTargetAsync and
--                  IComplianceEvaluationReadRepository.GetHistoryForTargetAsync. Additive-only
--                  evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ComplianceRulesEngine].[usp_ComplianceEvaluation_GetHistoryForTarget]
    @TenantId         UNIQUEIDENTIFIER,
    @TargetEntityType NVARCHAR(200),
    @TargetEntityId   UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT ComplianceEvaluationId, TenantId, SiteId, TargetEntityType, TargetEntityId, Status, StartedAtUtc, CompletedAtUtc, TriggeredBy
        FROM [ComplianceRulesEngine].[ComplianceEvaluation]
        WHERE TenantId = @TenantId AND TargetEntityType = @TargetEntityType AND TargetEntityId = @TargetEntityId AND IsDeleted = 0
        ORDER BY StartedAtUtc DESC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TargetEntityId=', @TargetEntityId, N'; User=', SYSTEM_USER);
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
