-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RulesEngine
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ComplianceRuleResult_Insert.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a single ComplianceRuleResult child row, with its violation details
--                  serialized as a JSON array in ViolationsJson. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ComplianceRulesEngine].[usp_ComplianceRuleResult_Insert]
    @ComplianceRuleResultId UNIQUEIDENTIFIER,
    @ComplianceEvaluationId UNIQUEIDENTIFIER,
    @RuleId                 UNIQUEIDENTIFIER,
    @RuleCode                NVARCHAR(200),
    @RuleName                NVARCHAR(200),
    @Category                NVARCHAR(30),
    @Passed                  BIT,
    @ViolationsJson          NVARCHAR(MAX),
    @EvaluatedAtUtc          DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [ComplianceRulesEngine].[ComplianceRuleResult]
        (ComplianceRuleResultId, ComplianceEvaluationId, RuleId, RuleCode, RuleName, Category, Passed, ViolationsJson, EvaluatedAtUtc)
        VALUES
        (@ComplianceRuleResultId, @ComplianceEvaluationId, @RuleId, @RuleCode, @RuleName, @Category, @Passed, @ViolationsJson, @EvaluatedAtUtc);
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ComplianceRuleResult', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ComplianceRuleResultId=', @ComplianceRuleResultId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ComplianceRuleResultId=', @ComplianceRuleResultId, N'; User=', SYSTEM_USER);
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
