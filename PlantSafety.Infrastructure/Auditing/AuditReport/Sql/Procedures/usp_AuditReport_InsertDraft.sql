-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditReport
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AuditReport_InsertDraft.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new AuditReport row in Draft status. Sections are inserted
--                  separately, one row per section, via usp_AuditReportSection_Insert within the
--                  same Infrastructure-layer transaction. Backs IAuditReportRepository.AddAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Auditing].[usp_AuditReport_InsertDraft]
    @AuditReportId    UNIQUEIDENTIFIER,
    @TenantId         UNIQUEIDENTIFIER,
    @SiteId           UNIQUEIDENTIFIER = NULL,
    @TargetEntityType NVARCHAR(200),
    @TargetEntityId   UNIQUEIDENTIFIER,
    @Format           NVARCHAR(10),
    @Status           NVARCHAR(20),
    @RequestedBy      UNIQUEIDENTIFIER,
    @RequestedAtUtc   DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Auditing].[AuditReport]
        (AuditReportId, TenantId, SiteId, TargetEntityType, TargetEntityId, Format, Status, RequestedBy, RequestedAtUtc)
        VALUES
        (@AuditReportId, @TenantId, @SiteId, @TargetEntityType, @TargetEntityId, @Format, @Status, @RequestedBy, @RequestedAtUtc);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'AuditReport', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AuditReportId=', @AuditReportId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AuditReportId=', @AuditReportId, N'; User=', SYSTEM_USER);
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
