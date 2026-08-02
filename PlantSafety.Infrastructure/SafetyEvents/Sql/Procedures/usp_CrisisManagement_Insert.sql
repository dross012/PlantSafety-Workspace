-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.CrisisManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CrisisManagement_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new CrisisManagement aggregate root row. Backs
--                  ICrisisManagementRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB logging
--                  per the canonical write-procedure template (mirrors usp_Permits_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_CrisisManagement_Insert]
    @CrisisManagementId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @Title NVARCHAR(200),
    @Severity NVARCHAR(50),
    @DeclaredAtUtc DATETIME2(3),
    @RespondedBy UNIQUEIDENTIFIER = NULL,
    @RespondedAtUtc DATETIME2(3) = NULL,
    @StabilizedAtUtc DATETIME2(3) = NULL,
    @ResolvedBy UNIQUEIDENTIFIER = NULL,
    @ResolvedAtUtc DATETIME2(3) = NULL,
    @ClosedBy UNIQUEIDENTIFIER = NULL,
    @ClosedAtUtc DATETIME2(3) = NULL,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [SafetyEvents].[CrisisManagement]
        (
            CrisisId, TenantId, SiteId, Status, Title, Severity, DeclaredAtUtc, DeclaredBy,
            RespondedBy, RespondedAtUtc, StabilizedAtUtc, ResolvedBy, ResolvedAtUtc,
            ClosedBy, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @CrisisManagementId, @TenantId, @SiteId, @Status, @Title, @Severity, @DeclaredAtUtc, @CreatedBy,
            @RespondedBy, @RespondedAtUtc, @StabilizedAtUtc, @ResolvedBy, @ResolvedAtUtc,
            @ClosedBy, @ClosedAtUtc, @ArchivedAtUtc, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'CrisisManagement', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CrisisManagementId=', @CrisisManagementId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CrisisManagementId=', @CrisisManagementId, N'; User=', SYSTEM_USER);
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
