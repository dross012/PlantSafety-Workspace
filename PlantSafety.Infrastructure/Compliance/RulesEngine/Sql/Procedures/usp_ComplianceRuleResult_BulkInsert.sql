-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RulesEngine
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ComplianceRuleResult_BulkInsert.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Performance Optimization Agent, Phase 5 §7.3)
--  SchemaVersion:  2026.1
--  Description:    Set-based replacement for the per-row usp_ComplianceRuleResult_Insert loop
--                  previously issued once per rule result from
--                  ComplianceEvaluationDapperRepository.AddAsync (a genuine N+1 write pattern --
--                  a single ComplianceEvaluation run can evaluate dozens to hundreds of rules
--                  against one target entity). Accepts every rule result for the evaluation as a
--                  single table-valued parameter and inserts them in one round trip.
--                  usp_ComplianceRuleResult_Insert.sql is left in place, unmodified, for any
--                  other caller. Table type + bulk-insert pattern mirrors the established
--                  PlanningProjects.PlanningProjectMilestoneTableType /
--                  usp_PlanningProject_Insert.sql precedent.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF TYPE_ID(N'[ComplianceRulesEngine].[ComplianceRuleResultTableType]') IS NULL
BEGIN
    EXEC('CREATE TYPE [ComplianceRulesEngine].[ComplianceRuleResultTableType] AS TABLE
    (
        ComplianceRuleResultId UNIQUEIDENTIFIER NOT NULL,
        RuleId                 UNIQUEIDENTIFIER NOT NULL,
        RuleCode                NVARCHAR(200)    NOT NULL,
        RuleName                NVARCHAR(200)    NOT NULL,
        Category                NVARCHAR(30)     NOT NULL,
        Passed                  BIT              NOT NULL,
        ViolationsJson          NVARCHAR(MAX)    NOT NULL,
        EvaluatedAtUtc          DATETIME2(3)     NOT NULL
    )');
END
GO

CREATE OR ALTER PROCEDURE [ComplianceRulesEngine].[usp_ComplianceRuleResult_BulkInsert]
    @ComplianceEvaluationId UNIQUEIDENTIFIER,
    @RuleResults [ComplianceRulesEngine].[ComplianceRuleResultTableType] READONLY
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [ComplianceRulesEngine].[ComplianceRuleResult]
        (ComplianceRuleResultId, ComplianceEvaluationId, RuleId, RuleCode, RuleName, Category, Passed, ViolationsJson, EvaluatedAtUtc)
        SELECT
            r.ComplianceRuleResultId, @ComplianceEvaluationId, r.RuleId, r.RuleCode, r.RuleName, r.Category, r.Passed, r.ViolationsJson, r.EvaluatedAtUtc
        FROM @RuleResults r;
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
