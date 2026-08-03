-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RulesEngine
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ComplianceEvaluation_GetById.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single ComplianceEvaluation row by identity, tenant-scoped.
--                  Backs both IComplianceEvaluationRepository.GetByIdAsync and
--                  IComplianceEvaluationReadRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ComplianceRulesEngine].[usp_ComplianceEvaluation_GetById]
    @ComplianceEvaluationId UNIQUEIDENTIFIER,
    @TenantId               UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT ComplianceEvaluationId, TenantId, SiteId, TargetEntityType, TargetEntityId, Status, StartedAtUtc, CompletedAtUtc, TriggeredBy
        FROM [ComplianceRulesEngine].[ComplianceEvaluation]
        WHERE ComplianceEvaluationId = @ComplianceEvaluationId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ComplianceEvaluationId=', @ComplianceEvaluationId, N'; User=', SYSTEM_USER);
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
