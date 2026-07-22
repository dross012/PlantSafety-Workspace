-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PermitTemplate_Insert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new PermitTemplate aggregate root row. Backs IPermitTemplateRepository.AddAsync.
--                  TRY/CATCH with AuditDB/ErrorDB logging per the canonical
--                  write-procedure template. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PermitToWork].[usp_PermitTemplate_Insert]
    @PermitTemplateId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @PermitTypeCode NVARCHAR(400),
    @RequiredControls NVARCHAR(400),
    @StandardValidityHours INT,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [PermitToWork].[PermitTemplate]
        (
            PermitTemplateId, TenantId, SiteId, Status, PermitTypeCode, RequiredControls, StandardValidityHours, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @PermitTemplateId, @TenantId, @SiteId, @Status, @PermitTypeCode, @RequiredControls, @StandardValidityHours, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PermitTemplate', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PermitTemplateId=', @PermitTemplateId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PermitTemplateId=', @PermitTemplateId, N'; User=', SYSTEM_USER);
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
