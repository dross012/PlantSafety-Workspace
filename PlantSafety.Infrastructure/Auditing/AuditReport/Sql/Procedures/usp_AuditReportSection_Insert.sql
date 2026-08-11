-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditReport
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AuditReportSection_Insert.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a single AuditReportSection child row. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Auditing].[usp_AuditReportSection_Insert]
    @AuditReportSectionId UNIQUEIDENTIFIER,
    @AuditReportId        UNIQUEIDENTIFIER,
    @SectionType           NVARCHAR(30),
    @PrintOrder            INT,
    @Heading                NVARCHAR(200),
    @ContentJson            NVARCHAR(MAX)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Auditing].[AuditReportSection]
        (AuditReportSectionId, AuditReportId, SectionType, PrintOrder, Heading, ContentJson)
        VALUES
        (@AuditReportSectionId, @AuditReportId, @SectionType, @PrintOrder, @Heading, @ContentJson);
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'AuditReportSection', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AuditReportSectionId=', @AuditReportSectionId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AuditReportSectionId=', @AuditReportSectionId, N'; User=', SYSTEM_USER);
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
