-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Compliance
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ComplianceRuleAggregate_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new (self-named) Compliance aggregate root row. Backs
--                  IComplianceRepository.AddAsync. Named "ComplianceRuleAggregate" (not bare
--                  "Compliance") to avoid ambiguity with the top-level Compliance module/schema
--                  and the pre-existing read-only category slices under this module folder.
--                  TRY/CATCH with AuditDB/ErrorDB logging per the canonical write-procedure
--                  template (mirrors usp_Permits_Insert.sql). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_ComplianceRuleAggregate_Insert]
    @ComplianceId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @RegulationReference NVARCHAR(400),
    @RuleDescription NVARCHAR(2000),
    @ComplianceScore DECIMAL(5,2),
    @NonConformanceCount INT,
    @ResponsiblePartyId UNIQUEIDENTIFIER,
    @LastEvaluatedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Compliance].[ComplianceRuleAggregate]
        (
            ComplianceId, TenantId, SiteId, Status, RegulationReference, RuleDescription,
            ComplianceScore, NonConformanceCount, ResponsiblePartyId, LastEvaluatedAtUtc, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @ComplianceId, @TenantId, @SiteId, @Status, @RegulationReference, @RuleDescription,
            @ComplianceScore, @NonConformanceCount, @ResponsiblePartyId, @LastEvaluatedAtUtc, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ComplianceRuleAggregate', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ComplianceId=', @ComplianceId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ComplianceId=', @ComplianceId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
