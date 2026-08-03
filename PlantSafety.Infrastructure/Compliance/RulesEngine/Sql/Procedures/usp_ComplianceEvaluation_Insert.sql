-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RulesEngine
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ComplianceEvaluation_Insert.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new ComplianceEvaluation aggregate root row. Rule results are
--                  inserted separately, one row per result, via usp_ComplianceRuleResult_Insert
--                  within the same Infrastructure-layer transaction. Backs
--                  IComplianceEvaluationRepository.AddAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ComplianceRulesEngine].[usp_ComplianceEvaluation_Insert]
    @ComplianceEvaluationId UNIQUEIDENTIFIER,
    @TenantId               UNIQUEIDENTIFIER,
    @SiteId                 UNIQUEIDENTIFIER = NULL,
    @TargetEntityType       NVARCHAR(200),
    @TargetEntityId         UNIQUEIDENTIFIER,
    @Status                 NVARCHAR(30),
    @StartedAtUtc           DATETIME2(3),
    @CompletedAtUtc         DATETIME2(3) = NULL,
    @TriggeredBy            UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [ComplianceRulesEngine].[ComplianceEvaluation]
        (ComplianceEvaluationId, TenantId, SiteId, TargetEntityType, TargetEntityId, Status, StartedAtUtc, CompletedAtUtc, TriggeredBy, IsDeleted)
        VALUES
        (@ComplianceEvaluationId, @TenantId, @SiteId, @TargetEntityType, @TargetEntityId, @Status, @StartedAtUtc, @CompletedAtUtc, @TriggeredBy, 0);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ComplianceEvaluation', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ComplianceEvaluationId=', @ComplianceEvaluationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
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
